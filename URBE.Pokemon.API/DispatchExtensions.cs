﻿using URBE.Pokemon.API.Models.Database;

namespace URBE.Pokemon.API;

public static class DispatchExtensions
{
    public static IQueryable<TDispatchable> GetPendingDispatchsFor<TDispatchable>(this IQueryable<TDispatchable> dispatchables, Server server)
        where TDispatchable : IDispatchable
    {
        var claimdelay = Program.Settings.DispatchModelAfterClaimDelay;
        var claimexpir = Program.Settings.DispatchModelClaimExpiration;
        var dtnow = DateTimeOffset.Now - claimdelay;
        return dispatchables.Where(x => x.DispatchedAt == null && x.ClaimedAt < dtnow && x.ClaimedBy == server);
    }

    public static Task ClaimDispatches<TDispatchable>(this IQueryable<TDispatchable> dispatchables, Server server, CancellationToken ct = default)
        where TDispatchable : IDispatchable
    {
        var claimexpir = Program.Settings.DispatchModelClaimExpiration;
        var dtnow = DateTimeOffset.Now;
        return dispatchables
            .Where(x => x.DispatchedAt == null && x.ClaimedAt + claimexpir < dtnow)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.ClaimedAt, dtnow).SetProperty(x => x.ClaimedBy, server), cancellationToken: ct);
    }
}
