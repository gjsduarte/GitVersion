﻿using System;
using GitVersion;
using LibGit2Sharp;
using NUnit.Framework;

[TestFixture]
public class RemoteRepositoryScenarios
{
    [Test]
    public void GivenARemoteGitRepositoryWithCommits_ThenClonedLocalShouldMatchRemoteVersion()
    {
        using (var fixture = new RemoteRepositoryFixture(new Config()))
        {
            fixture.AssertFullSemver("0.1.0+4");
            fixture.AssertFullSemver("0.1.0+4", fixture.LocalRepository);
        }
    }

    [Test]
    public void GivenARemoteGitRepositoryWithCommitsAndBranches_ThenClonedLocalShouldMatchRemoteVersion()
    {
        using (var fixture = new RemoteRepositoryFixture(
            path =>
            {
                Repository.Init(path);
                Console.WriteLine("Created git repository at '{0}'", path);

                var repo = new Repository(path);
                repo.MakeCommits(5);

                repo.CreateBranch("develop");
                repo.CreateBranch("release-1.0");

                repo.Checkout("release-1.0");
                repo.MakeCommits(5);

                return repo;
            },
            new Config()))
        {
            fixture.AssertFullSemver("1.0.0-beta.1+5");
            fixture.AssertFullSemver("1.0.0-beta.1+5", fixture.LocalRepository);
        }
    }

    [Test]
    public void GivenARemoteGitRepositoryAheadOfLocalRepository_ThenChangesShouldPull()
    {
        using (var fixture = new RemoteRepositoryFixture(new Config()))
        {
            fixture.Repository.MakeACommit();
            fixture.AssertFullSemver("0.1.0+5");
            fixture.AssertFullSemver("0.1.0+4", fixture.LocalRepository);
            fixture.LocalRepository.Network.Pull(fixture.LocalRepository.Config.BuildSignature(new DateTimeOffset(DateTime.Now)), new PullOptions());
            fixture.AssertFullSemver("0.1.0+5", fixture.LocalRepository);
        }
    }

    [Test]
    public void GivenARemoteGitRepositoryWhenCheckingOutDetachedhead_UsingExistingImplementationThrowsException()
    {

        using (var fixture = new RemoteRepositoryFixture(new Config()))
        {
            fixture.IsForTrackedBranchOnly = false;
            fixture.LocalRepository.Checkout(fixture.LocalRepository.Head.Tip);

            Assert.Throws<WarningException>(() => fixture.AssertFullSemver("0.1.0+4", fixture.LocalRepository), "It looks like the branch being examined is a detached Head pointing to commit '{0}'. Without a proper branch name GitVersion cannot determine the build version.", fixture.LocalRepository.Head.Tip.Id.ToString(7));
        }
    }

    [Test]
    public void GivenARemoteGitRepositoryWhenCheckingOutDetachedhead_UsingTrackingBranchOnlyBehaviourShouldReturnVersion_0_1_4plus5()
    {

        using (var fixture = new RemoteRepositoryFixture(new Config()))
        {

            fixture.LocalRepository.Checkout(fixture.LocalRepository.Head.Tip);

            fixture.AssertFullSemver("0.1.0+4", fixture.LocalRepository);
        }
    }
}