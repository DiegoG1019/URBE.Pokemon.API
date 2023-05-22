﻿using System.Diagnostics;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting.Server;
using MimeKit;
using URBE.Pokemon.API.Services;

namespace URBE.Pokemon.API.Workers;

//[RegisterUrbeWorker]
public class MailSender : ApiServiceWorker
{
    private static readonly string MCRTemplate = File.ReadAllText("Resources/MailConfirmationRequestMessageTemplate.html");

    public MailSender(IServiceProvider rootProvider) : base(rootProvider) { }

    public override async Task Work(CancellationToken stoppingToken)
    {
        var server = Program.Server;

        using var services = GetNewScopedServices();
        using var db = services.GetRequiredService<UrbeContext>();
        var (mail, sender) = await SmtpProvider.GetSmtpClient("MailConfirmationRequestSender", stoppingToken);
        using (mail)
        {
            Log.Debug("Looking for pending mail confirmation requests");
            foreach (var mcr in await db.MailConfirmationRequests
                .GetPendingDispatchsFor(server)
                .Include(x => x.User)
                .ToArrayAsync(stoppingToken))
            {
                Log.Debug("Processing a mail confirmation request for user {user} ({userid}) under email {email}", mcr.User.DisplayName, mcr.UserId, mcr.Email);
                var bb = new BodyBuilder
                {
                    HtmlBody = MCRTemplate.Replace("{{mcrlink}}", $"{Program.Settings.FrontFacingBaseAddress}/ConfirmMail/{mcr.Id}")
                };

                Debug.Assert(mcr.User is not null);
                Debug.Assert(mcr.User.Email is not null);
                if (mcr.User.Email is null)
                    throw new InvalidDataException("An user cannot have a null email if a mail confirmation request is being made");

                mcr.DispatchedAt = DateTimeOffset.Now; ;
                await db.SaveChangesAsync(stoppingToken);

                await mail.SendAsync(
                        new MimeMessage(
                            from: new InternetAddress[] { sender },
                            to: new InternetAddress[] { new MailboxAddress(mcr.User.DisplayName, mcr.Email ) },
                            subject: "Mail Confirmation",
                            body: bb.ToMessageBody()
                        ),
                        stoppingToken
                    );
            }
        }

        await db.MailConfirmationRequests.ClaimDispatches(server, stoppingToken);
    }
}
