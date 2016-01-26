using System.Collections.Generic;
using System.IO;

namespace POS
{
    public class Register
    {
        private const string ReadFailureMessage = "Read Failure";
        private const string BarcodeNotFoundMessage = "Barcode not found : \"{0}\"";
        private const string TotalPriceMessage = "Total: {0}";
        private const double DiscountForSecondItem = 0.5;
        private readonly Dictionary<string, int> _priceMapping;
        private int _totalPrice;
        private readonly TextWriter _textWriter;
        private readonly Dictionary<string, int> _cart;
        private readonly ISet<string> _getOneFreeItems;

        public Register(TextWriter textWriter)
        {
            _priceMapping = new Dictionary<string, int>();
            _totalPrice = 0;
            _textWriter = textWriter;
            _cart = new Dictionary<string, int>();
            _getOneFreeItems = new HashSet<string>();
        }


        public void Scan(string barCode)
        {
            var priceString = ScanInternal(barCode);
            _textWriter.WriteLine(priceString);
        }

        private string ScanInternal(string barCode)
        {
            if (IsCorruptBarcode(barCode))
            {
                return ReadFailureMessage;
            }
            if (IsUnknownBarcode(barCode))
            {
                return string.Format(BarcodeNotFoundMessage, barCode);
            }
            var cents = CalculatePrice(barCode);
            _totalPrice += cents;
            var formatCurrency = FormatCurrency(cents);
            return formatCurrency;
        }

        private int CalculatePrice(string barCode)
        {
            var cents = _priceMapping[barCode];
            if (IsInCart(barCode))
            {
                _cart[barCode]++;
                if (IsFree(barCode))
                {
                    cents = 0;
                }
                else
                {
                    cents = (int) (cents*DiscountForSecondItem);
                }
            }
            else
            {
                _cart.Add(barCode, 1);
            }
            return cents;
        }

        private bool IsFree(string barCode)
        {
            return _getOneFreeItems.Contains(barCode) && _cart[barCode] % 2 == 0;
        }

        private bool IsInCart(string barCode)
        {
            return _cart.ContainsKey(barCode);
        }

        private bool IsUnknownBarcode(string barCode)
        {
            return !_priceMapping.ContainsKey(barCode);
        }

        private static bool IsCorruptBarcode(string barCode)
        {
            return barCode.Equals("");
        }

        private static string FormatCurrency(int cents)
        {
            var price = cents/100.0;
            var priceString = ToEuroFormattedString(price);
            return priceString;
        }

        private static string ToEuroFormattedString(double price)
        {
            return price.ToString("C2");
        }

        public void Add(string barCode, int price, bool getOneFree = false)
        {
            _priceMapping.Add(barCode, price);
            if (getOneFree)
            {
                _getOneFreeItems.Add(barCode);
            }
        }

        public void Total()
        {
            var totalString = FormatCurrency(_totalPrice);
            _totalPrice = 0;
            _textWriter.WriteLine(TotalPriceMessage, totalString);
            _cart.Clear();
        }
    }
}