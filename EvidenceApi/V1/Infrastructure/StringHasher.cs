using System;
using System.Security.Cryptography;
using System.Text;
using EvidenceApi.V1.Infrastructure.Interfaces;

namespace EvidenceApi.V1.Infrastructure
{
    public class StringHasher : IStringHasher
    {
        private readonly HashAlgorithm _hashAlgorithm;

        public StringHasher()
        {
            _hashAlgorithm = SHA512.Create();
        }

        public string create(string toHash)
        {
            var hash = _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(toHash));
            return BitConverter.ToString(hash);
        }
    }
}
