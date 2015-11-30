using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    public class Register
    {
        private Dictionary<string, int> _priceMapping;
        private int _totalPrice;
        private TextWriter _textWriter;

        public Register(TextWriter textWriter)
        {
            _priceMapping = new Dictionary<string, int>();
            _totalPrice = 0;
            this._textWriter = textWriter;
        }
      

        public string Scan(string barCode)
        {
            var priceString = ScanInternal(barCode);
            _textWriter.WriteLine(priceString);
            return priceString;
        }

        private string ScanInternal(string barCode)
        {
            if (barCode.Equals(""))
            {
                return "Read Failure";
            }
            if (!_priceMapping.ContainsKey(barCode))
            {
                return "Barcode not found : \"" + barCode + "\"";
            }
            var cents = _priceMapping[barCode];
            _totalPrice += cents;
            var formatCurrency = FormatCurrency(cents);
            return formatCurrency;
        }

        private static string FormatCurrency(int cents)
        {
            var price = cents/100.0;
            var priceString = price.ToString("C2");
            return priceString;
        }

        public void Add(string barCode, int price)
        {
            _priceMapping.Add(barCode, price);
        }

        public string Total()
        {
            var totalString = FormatCurrency(_totalPrice);
            _totalPrice = 0;
            _textWriter.WriteLine("Total: " + totalString);
            return totalString;
        }
    }
}
