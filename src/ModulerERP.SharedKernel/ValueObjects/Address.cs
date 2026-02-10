using System.Collections.Generic;

namespace ModulerERP.SharedKernel.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string District { get; private set; } // İlçe
    public string City { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    
    // TRNC Specifics
    public string Block { get; private set; } // Ada
    public string Parcel { get; private set; } // Parsel

    public Address() { } // For EF Core

    public Address(string street, string district, string city, string zipCode, string country, string block = "", string parcel = "")
    {
        Street = street;
        District = district;
        City = city;
        ZipCode = zipCode;
        Country = country;
        Block = block;
        Parcel = parcel;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return District;
        yield return City;
        yield return ZipCode;
        yield return Country;
        yield return Block;
        yield return Parcel;
    }
}
