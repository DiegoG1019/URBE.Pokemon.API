using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;
using Microsoft.IO;
using Nito.AsyncEx;
using URBE.Pokemon.API.Storage;
using URBE.Pokemon.API.Storage.Data;

namespace URBE.Pokemon.API.Storage.Implementations;

public partial class FTPStorageProvider : IStorageProvider
{
    public const string ProviderName = "FTP";
    private const int TransferChunkSize = 1024;

    private static readonly FtpConfig NoCertificateConfig = new()
    {
        RetryAttempts = 5,
        EncryptionMode = FtpEncryptionMode.Auto,
        ValidateAnyCertificate = true
    };

    private static readonly FtpConfig DefaultConfig = new()
    {
        RetryAttempts = 5,
        EncryptionMode = FtpEncryptionMode.Auto
    };

    public string Provider { get; } = ProviderName;

    private readonly AsyncLock ftpprofilelock = new();
    private FtpProfile? ftpprofile;

    private string? Password { get; }
    private string? Username { get; }
    private string Host { get; }
    private ushort Port { get; }

    public string? Root { get; }

    public bool ValidateAnyCertificate { get; }

    public FTPStorageProvider(FTPProviderData data, string? root)
        : this(data.Password, data.Username, data.Host, data.Port, root, data.ValidateAnyCertificate) { }

    public FTPStorageProvider(string? password, string? username, string host, ushort port, string? root, bool validateAnyCertificate = false)
    {
        Password = password;
        Username = username;
        Host = host ?? throw new ArgumentNullException(nameof(host));
        Port = port;
        Root = root;

#if DEBUG
        ValidateAnyCertificate = true;
#else
    ValidateAnyCertificate = validateAnyCertificate;
#endif
    }

    public string PreparePath(string path)
        => (Root is not null && path.StartsWith(Root) ? path : Path.Combine(Root ?? "", path)).Replace('\\', '/');

    public void Dispose() => GC.SuppressFinalize(this);
}