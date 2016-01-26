using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace POS.Tests
{
    [TestFixture]
    internal class Test
    {
        private Register uut;
        private TextWriter textWriter;

        [SetUp]
        public void Setup()
        {
            textWriter = new StringWriter();
            uut = new Register(textWriter);
        }

        [Test]
        public void SuccessfulInput()
        {
            uut.Add("1", 100);
            uut.Scan("1");
            AssertLastLineEquals("1,00 €");
        }

        [Test]
        public void UnsuccessfulInput()
        {
            uut.Scan("fail");
            AssertLastLineEquals("Barcode not found : \"fail\"");
        }

        [Test]
        public void BarcodeReadError()
        {
            uut.Scan("");
            AssertLastLineEquals("Read Failure");
        }

        [Test]
        public void LessThanOneEuro()
        {
            uut.Add("50003", 2);
            uut.Scan("50003");
            AssertLastLineEquals("0,02 €");
        }

        [Test]
        public void MultipleItems()
        {
            uut.Add("2056", 2000);
            uut.Add("50003", 2);
            uut.Add("0055", 150);

            uut.Scan("2056");
            AssertLastLineEquals("20,00 €");
            uut.Scan("50003");
            AssertLastLineEquals("0,02 €");
            uut.Scan("0055");
            AssertLastLineEquals("1,50 €");
        }

        [Test]
        public void TotalAmount()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");
            uut.Total();
            AssertLastLineEquals("Total: 20,00 €");
        }

        [Test]
        public void MultipleScans()
        {
            uut.Add("2056", 2000);
            uut.Add("0055", 150);
            uut.Scan("2056");
            uut.Scan("0055");
            uut.Total();
            AssertLastLineEquals("Total: 21,50 €");
        }

        [Test]
        public void ResetAfterTotal()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");

            uut.Total();
            AssertLastLineEquals("Total: 20,00 €");
            uut.Total();
            AssertLastLineEquals("Total: 0,00 €");
        }

        [Test]
        public void MultipleTotals()
        {
            uut.Add("2056", 2000);
            uut.Add("2057", 4000);
            uut.Scan("2056");
            uut.Total();
            AssertLastLineEquals("Total: 20,00 €");
            uut.Scan("2057");
            uut.Total();
            AssertLastLineEquals("Total: 40,00 €");
        }

        [Test]
        public void MultipleScansWithFailure()
        {
            uut.Add("2056", 2000);
            uut.Add("0055", 150);
            uut.Scan("2056");
            uut.Scan("0055");
            uut.Scan("");
            uut.Scan("1337");
            uut.Total();
            AssertLastLineEquals("Total: 21,50 €");
        }

        [Test]
        public void DiscountForSecondItem()
        {
            uut.Add("1", 1000);
            uut.Scan("1");
            uut.Scan("1");
            AssertLastLineEquals("5,00 €");
        }

        [Test]
        public void DiscountForThirdItem()
        {
            uut.Add("1", 1000);
            uut.Scan("1");
            uut.Scan("1");
            uut.Scan("1");
            AssertLastLineEquals("5,00 €");
        }

        [Test]
        public void NoDiscountForSingleItem()
        {
            uut.Add("1", 1000);
            uut.Add("2", 1000);
            uut.Scan("1");
            uut.Scan("1");
            uut.Scan("2");
            AssertLastLineEquals("10,00 €");
        }

        [Test]
        public void NoDiscountAfterTotal()
        {
            uut.Add("1", 1000);
            uut.Scan("1");
            uut.Total();
            uut.Scan("1");
            AssertLastLineEquals("10,00 €");
        }

        [Test]
        public void BuyOneGetOneFree()
        {
            uut.Add("1", 1000, true);
            uut.Scan("1");
            uut.Scan("1");
            AssertLastLineEquals("0,00 €");
        }

        [Test]
        public void BuyOneGetOneFreeThirdTime()
        {
            uut.Add("1", 1000, true);
            uut.Scan("1");
            uut.Scan("1");
            uut.Scan("1");
            AssertLastLineNotEquals("0,00 €");
        }        
        
        [Test]
        public void BuyOneGetOneFreeFourthTime()
        {
            uut.Add("1", 1000, true);
            uut.Scan("1");
            uut.Scan("1");
            uut.Scan("1");
            uut.Scan("1");
            AssertLastLineEquals("0,00 €");
        }

        private void AssertLastLineNotEquals(string expected)
        {
            String[] newLineSeparator = { Environment.NewLine };
            Assert.AreNotEqual(expected,
                textWriter.ToString().Split(newLineSeparator, StringSplitOptions.RemoveEmptyEntries).Last());
        }

        private void AssertLastLineEquals(string expected)
        {
            String[] newLineSeparator = {Environment.NewLine};
            Assert.AreEqual(expected,
                textWriter.ToString().Split(newLineSeparator, StringSplitOptions.RemoveEmptyEntries).Last());
        }
    }
}