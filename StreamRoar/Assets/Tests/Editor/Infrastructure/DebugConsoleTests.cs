using System;
using System.Collections.Generic;
using NUnit.Framework;
using StreamRoar.Infrastructure;

namespace StreamRoar.Tests.Editor.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    public sealed class DebugConsoleTests
    {
        DebugConsole m_Console;

        [SetUp]
        public void SetUp()
        {
            m_Console = new DebugConsole();
            MetaConsoleCommands.RegisterAll(m_Console, m_Console.Registry);
        }

        [Test]
        public void Tokenizer_SplitsWhitespace_AndQuotedStrings()
        {
            Assert.That(ConsoleTokenizer.TryTokenize("echo hello world", out var a, out _), Is.True);
            Assert.That(a, Is.EqualTo(new[] { "echo", "hello", "world" }));

            Assert.That(ConsoleTokenizer.TryTokenize("echo \"hello world\"", out var b, out _), Is.True);
            Assert.That(b, Is.EqualTo(new[] { "echo", "hello world" }));

            Assert.That(ConsoleTokenizer.TryTokenize("echo \"a\\\"b\"", out var c, out _), Is.True);
            Assert.That(c, Is.EqualTo(new[] { "echo", "a\"b" }));
        }

        [Test]
        public void Tokenizer_UnclosedQuote_Fails()
        {
            Assert.That(ConsoleTokenizer.TryTokenize("echo \"oops", out _, out string error), Is.False);
            Assert.That(error, Does.Contain("quote").IgnoreCase);
        }

        [Test]
        public void Tokenizer_BlankLine_SucceedsEmpty()
        {
            Assert.That(ConsoleTokenizer.TryTokenize("   ", out var tokens, out _), Is.True);
            Assert.That(tokens, Is.Empty);
        }

        [Test]
        public void TryExecute_UnknownCommand_WritesError()
        {
            bool ok = m_Console.TryExecute("nope.cmd", out string error);
            Assert.That(ok, Is.False);
            Assert.That(error, Does.Contain("Unknown"));
            Assert.That(HasErrorContaining("Unknown"), Is.True);
        }

        [Test]
        public void TryExecute_Echo_WritesJoinedArgs()
        {
            Assert.That(m_Console.TryExecute("echo hi there", out _), Is.True);
            Assert.That(HasInfoContaining("hi there"), Is.True);
        }

        [Test]
        public void TryExecute_Help_ListsMetaCommands()
        {
            Assert.That(m_Console.TryExecute("help", out _), Is.True);
            Assert.That(HasInfoContaining("clear"), Is.True);
            Assert.That(HasInfoContaining("echo"), Is.True);
            Assert.That(HasInfoContaining("help"), Is.True);
        }

        [Test]
        public void TryExecute_HelpSpecific_ShowsSchema()
        {
            Assert.That(m_Console.TryExecute("help echo", out _), Is.True);
            Assert.That(HasInfoContaining("echo"), Is.True);
        }

        [Test]
        public void TryExecute_Clear_EmptiesOutput()
        {
            m_Console.TryExecute("echo keep", out _);
            Assert.That(m_Console.GetOutputSnapshot().Count, Is.GreaterThan(0));
            Assert.That(m_Console.TryExecute("clear", out _), Is.True);
            Assert.That(m_Console.GetOutputSnapshot().Count, Is.EqualTo(0));
        }

        [Test]
        public void TryExecute_WrongArity_Fails()
        {
            Assert.That(m_Console.TryExecute("clear extra", out string error), Is.False);
            Assert.That(error, Does.Contain("argument").IgnoreCase);
        }

        [Test]
        public void Register_CustomCommand_IsExecutable()
        {
            m_Console.Register(new DelegateConsoleCommand(
                "demo.ping",
                "ping",
                ConsoleArgSchema.None,
                (_, w) => w.WriteLine("pong")));

            Assert.That(m_Console.TryExecute("DEMO.PING", out _), Is.True);
            Assert.That(HasInfoContaining("pong"), Is.True);
        }

        [Test]
        public void Register_DuplicateName_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                m_Console.Register(new DelegateConsoleCommand(
                    "help",
                    "dup",
                    ConsoleArgSchema.None,
                    (_, __) => { })));
        }

        [Test]
        public void SuggestCompletion_UniquePrefix()
        {
            Assert.That(m_Console.SuggestCompletion("cl"), Is.EqualTo("clear"));
        }

        [Test]
        public void SuggestCompletion_DoesNotAllocateUnboundedCapacity()
        {
            // 回归：旧实现 new List(int.MaxValue) 会在 Tab 时 OOM。
            Assert.DoesNotThrow(() => m_Console.SuggestCompletion("h"));
            Assert.That(m_Console.SuggestCompletion("h"), Is.EqualTo("help"));
        }

        [Test]
        public void GetCompletionCandidates_He_IncludesHelp()
        {
            IReadOnlyList<string> candidates = m_Console.GetCompletionCandidates("he");
            Assert.That(candidates, Does.Contain("help"));
            Assert.That(m_Console.TryGetCommandHelp("help", out string help), Is.True);
            Assert.That(help, Does.Contain("List").IgnoreCase);
        }

        [Test]
        public void GetCompletionCandidates_AfterSpace_Empty()
        {
            Assert.That(m_Console.GetCompletionCandidates("help "), Is.Empty);
        }

        [Test]
        public void SuggestCompletion_SelectedIndex_AcceptsCandidate()
        {
            m_Console.Register(new DelegateConsoleCommand(
                "heat",
                "demo",
                ConsoleArgSchema.None,
                (_, __) => { }));

            var candidates = m_Console.GetCompletionCandidates("he");
            Assert.That(candidates.Count, Is.GreaterThanOrEqualTo(2));
            int heatIndex = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] == "heat")
                {
                    heatIndex = i;
                    break;
                }
            }

            Assert.That(heatIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(m_Console.SuggestCompletion("he", heatIndex), Is.EqualTo("heat"));
        }

        [Test]
        public void History_PreviousReturnsLastCommand()
        {
            m_Console.TryExecute("echo one", out _);
            m_Console.TryExecute("echo two", out _);
            Assert.That(m_Console.TryHistoryPrevious(out string line), Is.True);
            Assert.That(line, Is.EqualTo("echo two"));
            Assert.That(m_Console.TryHistoryPrevious(out line), Is.True);
            Assert.That(line, Is.EqualTo("echo one"));
        }

        [Test]
        public void OpenClose_Toggle()
        {
            Assert.That(m_Console.IsOpen, Is.False);
            m_Console.Open();
            Assert.That(m_Console.IsOpen, Is.True);
            m_Console.Toggle();
            Assert.That(m_Console.IsOpen, Is.False);
        }

        [Test]
        public void Noop_TryExecute_Fails()
        {
            var noop = new NoopDebugConsole();
            Assert.That(noop.TryExecute("echo x", out string error), Is.False);
            Assert.That(error, Does.Contain("disabled").IgnoreCase);
        }

        bool HasInfoContaining(string fragment)
        {
            foreach (ConsoleOutputEntry entry in m_Console.GetOutputSnapshot())
            {
                if (entry.Kind == ConsoleOutputKind.Info &&
                    entry.Text.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        bool HasErrorContaining(string fragment)
        {
            foreach (ConsoleOutputEntry entry in m_Console.GetOutputSnapshot())
            {
                if (entry.Kind == ConsoleOutputKind.Error &&
                    entry.Text.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
