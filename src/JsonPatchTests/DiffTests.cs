﻿using System.IO;
using System.Linq;
using JsonDiffPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Tavis.JsonPatch.Tests
{
    [TestFixture]
    public class DiffTests
    {
        [TestCase("{a:1, b:2, c:3}",
            "{a:1, b:2}",
            ExpectedResult = "[{\"op\":\"remove\",\"path\":\"/c\"}]",
            TestName = "JsonPatch remove works for a simple value")]

        [TestCase("{a:1, b:2, c:{d:1,e:2}}",
            "{a:1, b:2}",
            ExpectedResult = "[{\"op\":\"remove\",\"path\":\"/c\"}]",
            TestName = "JsonPatch remove works for a complex value")]

        [TestCase("{a:1, b:2}",
            "{a:1, b:2, c:3}",
            ExpectedResult = "[{\"op\":\"add\",\"path\":\"/c\",\"value\":3}]",
            TestName = "JsonPatch add works for a simple value")]

        [TestCase("{a:1, b:2}",
            "{a:1, b:2, c:{d:1,e:2}}",
            ExpectedResult = "[{\"op\":\"add\",\"path\":\"/c\",\"value\":{\"d\":1,\"e\":2}}]",
            TestName = "JsonPatch add works for a complex value")]

        [TestCase("{a:1, b:2, c:3}",
            "{a:1, b:2, c:4}",
            ExpectedResult = "[{\"op\":\"replace\",\"path\":\"/c\",\"value\":4}]",
            TestName = "JsonPatch replace works for int")]

        [TestCase("{a:1, b:2, c:\"foo\"}",
            "{a:1, b:2, c:\"bar\"}",
            ExpectedResult = "[{\"op\":\"replace\",\"path\":\"/c\",\"value\":\"bar\"}]",
            TestName = "JsonPatch replace works for string")]

        [TestCase("{a:1, b:2, c:{foo:1}}",
            "{a:1, b:2, c:{bar:2}}",
            ExpectedResult = "[{\"op\":\"remove\",\"path\":\"/c/foo\"},{\"op\":\"add\",\"path\":\"/c/bar\",\"value\":2}]",
            TestName = "JsonPatch replace works for object")]

        [TestCase("{a:1, b:2, c:3}",
            "{c:3, b:2, a:1}",
            ExpectedResult = "[]",
            TestName = "JsonPatch order does not matter")]

        [TestCase("{a:{b:{c:{d:1}}}}",
            "{a:{b:{d:{c:1}}}}",
            ExpectedResult = "[{\"op\":\"remove\",\"path\":\"/a/b/c\"},{\"op\":\"add\",\"path\":\"/a/b/d\",\"value\":{\"c\":1}}]",
            TestName = "JsonPatch handles deep nesting")]

        [TestCase("[1,2,3,4]",
            "[5,6,7]",
            ExpectedResult = "[{\"op\":\"replace\",\"path\":\"/\",\"value\":[5,6,7]}]",
            TestName = "JsonPatch handles a simple array and replaces it")]

        [TestCase("{a:[1,2,3,4]}",
            "{a:[5,6,7]}",
            ExpectedResult = "[{\"op\":\"replace\",\"path\":\"/a\",\"value\":[5,6,7]}]",
            TestName = "JsonPatch handles a simple array under a property and replaces it")]

        [TestCase("{a:[1,2,3,4]}",
            "{a:[1,2,3,4]}",
            ExpectedResult = "[]",
            TestName = "JsonPatch handles same array")]
        [TestCase("{a:[1,2,3,{name:'a'}]}",
            "{a:[1,2,3,{name:'a'}]}",
            ExpectedResult = "[]",
            TestName = "JsonPatch handles same array containing objects")]
        [TestCase("{a:[1,2,3,{name:'a'},4,5]}",
          "{a:[1,2,3,{name:'b'},4,5]}",
          ExpectedResult = "[{\"op\":\"replace\",\"path\":\"/a/3\",\"value\":{\"name\":\"b\"}}]",
          TestName = "Replaces array items")]
        //[TestCase("{a:[1,2,3,{name:'a'}]}",
        //    "{a:[1,2,3,{name:'b'}]}",
        //    ExpectedResult = "[{\"op\":\"replace\",\"path\":\"/a/3/name\",\"value\":'b'}]",
        //    TestName = "JsonPatch handles same array containing different objects")]
        public string JsonPatchesWorks(string leftString, string rightString)
        {
            var left = JToken.Parse(leftString);
            var right = JToken.Parse(rightString);

            var patchDoc = new JsonDiffer().Diff(left, right, false);
            var patcher = new JsonPatcher();
            patcher.Patch(ref left, patchDoc);


            Assert.True(JToken.DeepEquals(left, right));
            //var patchedLeft = left.ToString(Formatting.None);
            //var expected = right.ToString(Formatting.None);
            //Assert.AreEqual(expected, patchedLeft);

            var patchJson = patchDoc.ToString(Formatting.None);
            return patchJson;
        }

        [Test]
        public void ComplexExampleWithDeepArrayChange()
        {
            var scene1 = JToken.Parse(File.ReadAllText(@".\samples\scene1.json"));
            var scene2 = JToken.Parse(File.ReadAllText(@".\samples\scene2.json"));
            var patchDoc = new JsonDiffer().Diff(scene1, scene2, true);
            Assert.AreEqual("[{\"op\":\"remove\",\"path\":\"/entities/1\"}]", patchDoc.ToString(Formatting.None));
        }


    }
}