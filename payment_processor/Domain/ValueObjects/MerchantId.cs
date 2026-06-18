using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public sealed record MerchantId
    {
        public string Value { get; }

        public MerchantId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("merchant.id.required", "MerchantId is required.");

            value = value.Trim();

            if (value.Length > 50)
                throw new DomainException("merchant.id.invalid_length", "MerchantId length is invalid.");

            if (!Regex.IsMatch(value, "^[A-Za-z0-9_-]+$"))
                throw new DomainException("merchant.id.invalid_format", "MerchantId format is invalid.");

            Value = value;
        }

        public override string ToString() => Value;
    }
}
