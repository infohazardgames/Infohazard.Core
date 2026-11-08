// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_TagMask {
        private string[] _savedOverrideTags;

        [SetUp]
        public void SetUp() {
            // A project may override the tags (e.g. a generated GameTag.cs). Test against the default tags,
            // and restore the override afterwards. Leaving it cleared makes TagsGenerator prompt to regenerate.
            _savedOverrideTags = Tag.GameOverrideTags;
            Tag.GameOverrideTags = null;
        }

        [TearDown]
        public void TearDown() {
            Tag.GameOverrideTags = _savedOverrideTags;
        }

        private static readonly object[] DefaultTagMasks = {
            new object[] { Tag.Untagged, TagMask.UntaggedMask },
            new object[] { Tag.Respawn, TagMask.RespawnMask },
            new object[] { Tag.Finish, TagMask.FinishMask },
            new object[] { Tag.EditorOnly, TagMask.EditorOnlyMask },
            new object[] { Tag.MainCamera, TagMask.MainCameraMask },
            new object[] { Tag.Player, TagMask.PlayerMask },
            new object[] { Tag.GameController, TagMask.GameControllerMask },
        };

        [TestCaseSource(nameof(DefaultTagMasks))]
        public void MaskConstants_MatchGetMask(string tag, long mask) {
            Assert.That(TagMask.GetMask(tag), Is.EqualTo(mask));
        }

        [Test]
        public void NameToTagAndTagToName_RoundTrip() {
            for (int i = 0; i < Tag.DefaultTags.Length; i++) {
                Assert.That(TagMask.NameToTag(TagMask.TagToName(i)), Is.EqualTo(i));
            }
        }

        [Test]
        public void NameToTag_Unknown_ReturnsMinusOne() {
            Assert.That(TagMask.NameToTag("NotATag"), Is.EqualTo(-1));
        }

        [Test]
        public void GetMask_Unknown_LogsErrorAndReturnsZero() {
            LogAssert.Expect(LogType.Error, "Tag name NotATag not found.");
            Assert.That(TagMask.GetMask("NotATag"), Is.Zero);
        }

        [Test]
        public void GetMask_Multiple_CombinesAndSkipsUnknown() {
            LogAssert.Expect(LogType.Error, "Tag name NotATag not found.");
            long mask = TagMask.GetMask(Tag.Player, "NotATag", Tag.Finish);
            Assert.That(mask, Is.EqualTo(TagMask.PlayerMask | TagMask.FinishMask));
        }

        [Test]
        public void Operators_CombineMasks() {
            TagMask player = TagMask.PlayerMask;
            TagMask finish = TagMask.FinishMask;
            TagMask both = player | finish;

            Assert.That((long) both, Is.EqualTo(TagMask.PlayerMask | TagMask.FinishMask));
            Assert.That((long) (both & player), Is.EqualTo(TagMask.PlayerMask));
            Assert.That((long) (both ^ player), Is.EqualTo(TagMask.FinishMask));
            Assert.That((long) ~player, Is.EqualTo(~TagMask.PlayerMask));

            Assert.That((long) (both & TagMask.PlayerMask), Is.EqualTo(TagMask.PlayerMask));
            Assert.That(TagMask.PlayerMask & both, Is.EqualTo(TagMask.PlayerMask));
            Assert.That((long) (player | TagMask.FinishMask), Is.EqualTo((long) both));
            Assert.That(TagMask.FinishMask | player, Is.EqualTo((long) both));
            Assert.That((long) (both ^ TagMask.PlayerMask), Is.EqualTo(TagMask.FinishMask));
            Assert.That(TagMask.PlayerMask ^ both, Is.EqualTo(TagMask.FinishMask));
        }

        [Test]
        public void Equality_ComparesValue() {
            var a = new TagMask(TagMask.PlayerMask);
            var b = new TagMask(TagMask.PlayerMask);
            var c = new TagMask(TagMask.FinishMask);

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a.Equals((object) b), Is.True);
            Assert.That(a.Equals(c), Is.False);
            Assert.That(a.Equals("not a mask"), Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
            Assert.That(a.Value, Is.EqualTo(TagMask.PlayerMask));
        }

        [Test]
        public void ToStringArray_ReturnsSetTagsInOrder() {
            var mask = new TagMask(TagMask.PlayerMask | TagMask.UntaggedMask);
            Assert.That(mask.ToStringArray(), Is.EqualTo(new[] { Tag.Untagged, Tag.Player }));
            Assert.That(new TagMask(0).ToStringArray(), Is.Empty);
        }

        [TestCase(TagMask.UntaggedMask | TagMask.PlayerMask, "Untagged, Player")]
        [TestCase(TagMask.FinishMask, "Finish")]
        [TestCase(0L, "")]
        public void ToString_JoinsTagsWithCommas(long value, string expected) {
            Assert.That(new TagMask(value).ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void GameOverrideTags_ReplaceDefaultTags() {
            Tag.GameOverrideTags = new[] { "Alpha", "Beta" };
            Assert.That(Tag.Tags, Is.SameAs(Tag.GameOverrideTags));
            Assert.That(TagMask.GetMask("Beta"), Is.EqualTo(2));
            Assert.That(new TagMask(3).ToStringArray(), Is.EqualTo(new[] { "Alpha", "Beta" }));
        }

        [Test]
        [Ignore("Bug #5: TagMask uses int shifts on a long mask, so tags at index 32+ alias to lower bits.")]
        public void GameOverrideTags_IndexAbove31_UsesHighBits() {
            Tag.GameOverrideTags = Enumerable.Range(0, 40).Select(i => $"Tag{i}").ToArray();
            Assert.That(TagMask.GetMask("Tag35"), Is.EqualTo(1L << 35));
            Assert.That(TagMask.GetMask("Tag31"), Is.EqualTo(1L << 31));
            Assert.That(new TagMask(1L << 35).ToStringArray(), Is.EqualTo(new[] { "Tag35" }));
        }
    }
}
