using System.Reflection;
using LibGit2Sharp;

namespace web.Services;

public static class AutoCommiter
{
    public static bool AutoCommit()
    {
        var repoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            "../../../..");
        using (var repo = new Repository(repoPath))
        {
            var status = repo.RetrieveStatus();

            if (status.IsDirty)
            {
                Commands.Stage(repo, "*");
                var author = new Signature("AutoCommit", "AutoCommit@example.com", DateTimeOffset.Now);
                repo.Commit("AutoCommitOnStart " + DateTime.Now, author, author);
                repo.Network.Push(repo.Head);
            }
        }

        return true;
    }
}