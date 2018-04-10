using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShipEngine.ApiClient.Api;
using ShipEngine.ApiClient.Model;

namespace ShipEngineSampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var shipment = new AddressValidatingShipment
            {
                Confirmation = AddressValidatingShipment.ConfirmationEnum.None,
                CarrierId = null,
                ServiceCode = null,

                Packages = new List<ShipmentPackage>
                {
                    new ShipmentPackage
                    {
                        Weight = new Weight(8, Weight.UnitEnum.Ounce),
                        Dimensions = new Dimensions(Dimensions.UnitEnum.Inch, 5, 5, 5)
                    }
                }
            };

            var apiKey = GetApiKey();
            var carrier = ChooseCarrier(apiKey);

            shipment.ShipFrom = GetAddress(
                "Where are you shipping FROM?",
                new AddressDTO("John Doe", "5551234567", "Acme Corp.", "100 Main St.", null, null, "Austin", "TX", "78610", "US")
            );

            shipment.ShipTo = GetAddress(
                "Where are you shipping TO?",
                new AddressDTO("John Doe", "5551234567", "Acme Corp.", "100 Main St.", null, null, "Houston", "TX", "77002", "US")
            );

            var rate = ChooseRate(shipment, carrier, apiKey);

            Console.WriteLine("Purchasing label...");
            var isTestLabel = rate.CarrierCode == "stamps_com" || rate.CarrierCode == "endicia";
            var labelsApi = new LabelsApi();
            var label = labelsApi.LabelsPurchaseLabelWithRate(rate.RateId, new PurchaseLabelWithoutShipmentRequest(isTestLabel), apiKey);

            Console.WriteLine("Download your label:");
            Console.WriteLine(label.LabelDownload.Href);

            WaitToQuit();
        }

        static void WaitToQuit()
        {
            Console.WriteLine("\n\nPress any key to exit...");
            Console.Read();
            Environment.Exit(1);
        }

        static string GetApiKey()
        {
            Console.WriteLine("What's your API key?");
            var apiKey = Console.ReadLine();
            if (String.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = "ElJkhJuQIRoFq/kDEblco4LpZqRCdYNIoAVG7SywSXw";
                Console.WriteLine($"Defaulting to {apiKey}");
            }

            Console.WriteLine();
            return apiKey;
        }

        static Carrier ChooseCarrier(string apiKey)
        {
            Console.WriteLine("Getting carriers...");

            var carriersApi = new CarriersApi();
            var carriers = carriersApi.CarriersList(apiKey).Carriers;

            Console.WriteLine($"Looks like you've got {carriers.Count} carriers setup.");
            for (var i = 0; i < carriers.Count; i++)
            {
                var iCarrier = carriers[i];
                Console.WriteLine($"\t{i + 1}) {iCarrier.FriendlyName} ({iCarrier.CarrierId})");
            }

            Console.Write("\nChoose a carrier: ");
            var selectionRaw = Console.ReadLine();
            var selection = -1;
            if (selectionRaw == null || !int.TryParse(selectionRaw, out selection))
            {
                Console.WriteLine("No!");
                WaitToQuit();
            }

            var carrier = carriers[selection - 1];
            Console.WriteLine($"You selected {carrier.FriendlyName} ({carrier.CarrierId}).");
            Console.WriteLine();

            return carrier;
        }

        static Rate ChooseRate(AddressValidatingShipment shipment, Carrier carrier, string apiKey)
        {
            Console.WriteLine("Getting rates...");

            var ratesApi = new RatesApi();
            var rateShipmentRequest = new RateShipmentRequest(shipment: shipment, rateOptions: new RateRequest(new List<string> { carrier.CarrierId }));
            var rates = ratesApi.RatesRateShipment(rateShipmentRequest, apiKey).RateResponse.Rates;

            Console.WriteLine("Let's choose a rate:");

            for (var i = 0; i < rates.Count; i++)
            {
                Console.WriteLine($"\t{i + 1}) {rates[i].ServiceCode} - ${rates[i].ShippingAmount.Amount}");
            }

            Console.Write("\nChoose a rate: ");

            var selectionRaw = Console.ReadLine();
            var selection = -1;
            if (selectionRaw == null || !int.TryParse(selectionRaw, out selection))
            {
                Console.WriteLine("No!");
                WaitToQuit();
            }

            Console.WriteLine($"You selected {rates[selection - 1].ServiceCode}.");
            Console.WriteLine();

            return rates[selection - 1];
        }

        static AddressDTO GetAddress(string displayText, AddressDTO defaultAddress)
        {
            Console.WriteLine($"\n{displayText}");
            var address = new AddressDTO();

            Console.Write("Name: ");
            address.Name = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(address.Name))
            {
                address = defaultAddress;
                Console.WriteLine($"Defaulting to {address.AddressLine1}, {address.CityLocality}, {address.StateProvince} {address.PostalCode}");
            }
            else
            {
                Console.Write("Company: ");
                address.CompanyName = Console.ReadLine();

                Console.Write("Address (line 1): ");
                address.AddressLine1 = Console.ReadLine();

                Console.Write("Address (line 2): ");
                address.AddressLine2 = Console.ReadLine();

                Console.Write("City/Locality: ");
                address.CityLocality = Console.ReadLine();

                Console.Write("State/Province: ");
                address.StateProvince = Console.ReadLine();

                Console.Write("Postal Code: ");
                address.PostalCode = Console.ReadLine();

                Console.Write("Country Code: ");
                address.CountryCode = Console.ReadLine();

                Console.Write("Phone: ");
                address.Phone = Console.ReadLine();
            }

            Console.WriteLine();
            return address;
        }
    }
}
