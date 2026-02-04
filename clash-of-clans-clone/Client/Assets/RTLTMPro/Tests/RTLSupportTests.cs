//using System.Collections;
//using System.Collections.Generic;
//using System.Security.Policy;
//using NUnit.Framework;
//using RTLTMPro;
//using UnityEngine;
//using UnityEngine.TestTools;

//namespace Tests
//{
//    public class RTLSupportTests
//    {
//        [Test]
//        public void ArabicTextIsSuccessfulyConverted()
//        {
//            const string input = "????? ?????? ??????";
//            const string expected = "?????? ????? ?????";

//            string result = RTLSupport.FixRTL(input, false, false, false);

//            Assert.AreEqual(expected, result);
//        }

//        [Test]
//        public void FarsiTextIsSuccessfulyConverted()
//        {
//            const string input = "??? ?????";
//            const string expected = "????? ???";

//            string result = RTLSupport.FixRTL(input, false, false, true);

//            Assert.AreEqual(expected, result);
//        }

//        [Test]
//        public void TashkeelIsMaintainedInBeginingOfText()
//        {
//            const string input = "????";
//            const string expected = "????";;

//            string result = RTLSupport.FixRTL(input, false, false, false);

//            Assert.AreEqual(expected, result);
//        }
        
//        [Test]
//        public void TashkeelIsMaintainedInMiddleOfText()
//        {
//            const string input = "????";
//            const string expected = "????";

//            string result = RTLSupport.FixRTL(input, false, false, false);

//            Assert.AreEqual(expected, result);
//        }
        
//        [Test]
//        public void TashkeelIsMaintainedInEndOfText()
//        {
//            const string input = "????";
//            const string expected = "????";

//            string result = RTLSupport.FixRTL(input, false, false, false);

//            Assert.AreEqual(expected, result);
//        }
//    }
//}