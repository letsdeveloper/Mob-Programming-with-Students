
using System;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace POS.Tests
{
    [TestFixture]
    internal class Test
    {
        Register uut;
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
            Assert.AreEqual("1,00 €", uut.Scan("1"));
        }

        [Test]
        public void UnsuccessfulInput()
        {
            Assert.AreEqual("Barcode not found : \"fail\"", uut.Scan("fail"));
        }

        [Test]
        public void BarcodeReadError()
        {
            Assert.AreEqual("Read Failure",uut.Scan(""));
        }

        [Test]
        public void LessThanOneEuro()
        {
            uut.Add("50003", 2);
            Assert.AreEqual("0,02 €", uut.Scan("50003"));
        }

        [Test]
        public void MultipleItems()
        {
            uut.Add("2056", 2000);
            uut.Add("50003", 2);
            uut.Add("0055", 150);

            Assert.AreEqual("20,00 €", uut.Scan("2056"));
            Assert.AreEqual("0,02 €", uut.Scan("50003"));
            Assert.AreEqual("1,50 €", uut.Scan("0055"));
        }

        [Test]
        public void TotalAmount()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");
            Assert.AreEqual("20,00 €", uut.Total());
        }

        [Test]
        public void MultipleScans()
        {
            uut.Add("2056", 2000);
            uut.Add("0055", 150);
            uut.Scan("2056");
            uut.Scan("0055");
            uut.Scan("0055");
            Assert.AreEqual("23,00 €", uut.Total());
        }

        [Test]
        public void ResetAfterTotal()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");
            Assert.AreEqual("20,00 €", uut.Total());
            Assert.AreEqual("0,00 €", uut.Total());
        }

        [Test]
        public void MultipleTotals()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");
            Assert.AreEqual("20,00 €", uut.Total());
            uut.Scan("2056");
            uut.Scan("2056");
            Assert.AreEqual("40,00 €", uut.Total());
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
            uut.Scan("0055");
            Assert.AreEqual("23,00 €", uut.Total());
        }

        [Test]
        public void LiveOutputOnScan()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");
            Assert.AreEqual("20,00 €" + Environment.NewLine, textWriter.ToString());
        }

        [Test]
        public void LiveOutputOnTotal()
        {
            uut.Total();
            Assert.AreEqual("Total: 0,00 €" + Environment.NewLine
                , textWriter.ToString());
        }

        [Test]
        public void LiveOutputOnScanFailure()
        {
            uut.Scan("");
            Assert.AreEqual("Read Failure" + Environment.NewLine, textWriter.ToString());
        }

        [Test]
        public void LiveOutputOnBarcodeNotFound()
        {
            uut.Scan("uiae");
            Assert.AreEqual("Barcode not found : \"uiae\"" + Environment.NewLine, textWriter.ToString());
        }

        [Test]
        public void LiveOutputWithMultipleScansAndTotal()
        {
            uut.Add("2056", 2000);
            uut.Scan("2056");
            uut.Scan("2056");
            uut.Total();
            uut.Scan("");
            uut.Scan("qwerf");
            var sb = new StringBuilder();
            sb.AppendLine("20,00 €");
            sb.AppendLine("20,00 €");
            sb.AppendLine("Total: 40,00 €");
            sb.AppendLine("Read Failure");
            sb.AppendLine("Barcode not found : \"qwerf\"");
            Assert.AreEqual(sb.ToString(), textWriter.ToString());
        }
    }
}