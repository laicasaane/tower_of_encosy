using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EncosyTower.Encryption
{
    public static class RandomStringGenerator
    {
        public static string Generate(int length)
        {
            const string CHARSET = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            const int RANDOM_LENGTH = 16;

            using var cryptoProvider = new RNGCryptoServiceProvider();

            var result = new StringBuilder();
            var remainingLength = length;
            var randomNumberHolder = new byte[1];
            var randomNumbers = new List<int>(RANDOM_LENGTH);
            var charSet = CHARSET.AsSpan();

            while (remainingLength > 0)
            {
                randomNumbers.Clear();

                for (var randomCount = 0; randomCount < RANDOM_LENGTH; randomCount++)
                {
                    cryptoProvider.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomIndex = 0; randomIndex < randomNumbers.Count; randomIndex++)
                {
                    if (remainingLength == 0)
                    {
                        break;
                    }

                    var randomNumber = randomNumbers[randomIndex];

                    if (randomNumber < charSet.Length)
                    {
                        result.Append(charSet[randomNumber]);
                        remainingLength--;
                    }
                }
            }

            return result.ToString();
        }
    }
}
