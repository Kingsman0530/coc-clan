﻿using ClashOfClans.Models;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hyperstellar.Discord.Attr;
using Hyperstellar.Sql;
using Hyperstellar.Clash;
using Discord;

namespace Hyperstellar.Discord;

public class Cmds : InteractionModuleBase
{
    [RequireOwner]
    [SlashCommand("shutdown", "Shuts down the bot")]
    public async Task ShutdownAsync(bool commit = true)
    {
        await RespondAsync("Ok", ephemeral: true);
        if (commit)
        {
            Db.Commit();
        }
        Environment.Exit(0);
    }

    [RequireOwner]
    [SlashCommand("commit", "Commits db")]
    public async Task CommitAsync()
    {
        Db.Commit();
        await RespondAsync("Committed", ephemeral: true);
    }

    [RequireOwner]
    [SlashCommand("admin", "Makes the Discord user an admin")] // Maybe rename to addadmin
    public async Task AdminAsync(SocketGuildUser user)
    {
        bool success = Db.AddAdmin(user.Id);
        if (success)
        {
            await RespondAsync("Success!", ephemeral: true);
        }
        else
        {
            await RespondAsync("Error", ephemeral: true);
        }
    }

    [RequireAdmin]
    [SlashCommand("alt", "Links an alt to a main")]
    public async Task AltAsync(Member alt, Member main)
    {
        if (alt.CocId == main.CocId)
        {
            await RespondAsync("Bro alt must be different from main bruh");
            return;
        }
        if (main.IsAlt())
        {
            await RespondAsync("Main can't be an alt in the database!");
            return;
        }
        if (alt.IsAltMain())
        {
            await RespondAsync("Alt can't be a main in the database!");
            return;
        }

        main.AddAlt(alt);
        ClanMember clanAlt = Coc.GetMember(alt.CocId);
        ClanMember clanMain = Coc.GetMember(main.CocId);
        await RespondAsync($"`{clanAlt.Name}` is now an alt of `{clanMain.Name}`");
    }

    [SlashCommand("info", "Print the member's infomation")]
    public async Task InfoAsync(Member member)
    {
        ClanMember cocMem = Coc.GetMember(member.CocId);

        var embed = new EmbedBuilder
        {
            Title = cocMem.Name,
            Author = new EmbedAuthorBuilder
            {
                Name = cocMem.Tag,
            },

        };

        await RespondAsync(embed: embed.Build());

    [RequireAdmin]
    [SlashCommand("link", "[Admin] Links a Discord account to a Main")]
    public async Task LinkAsync(Member coc, IGuildUser discord)
    {
        Main? main = coc.TryToMain();
        if (main == null)
        {
            await RespondAsync("`coc` can't be an alt!");
            return;
        }
        if (discord.IsBot)
        {
            await RespondAsync("`discord` can't be a bot!");
            return;
        }
        main.Discord = discord.Id;
        main.Update();
        await RespondAsync("Linked");
    }
}
