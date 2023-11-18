using System;
using System.Net;
using System.Threading;
using nanoFramework.TestFramework;

namespace CCSWE.nanoFramework.DhcpServer.UnitTests
{
    [TestClass]
    public class IPAddressPoolTests
    {
        private static IPAddress GenerateAddress(byte lastOctet) => new(new byte[] { 192, 168, 4, lastOctet });

        [TestMethod]
        public void Evict_removes_expired_leases()
        {
            var sut = new IPAddressPool(GenerateAddress(1));

            var available1 = sut.Available;
            Console.WriteLine($"available1 = {available1}");
            for (byte i = 1; i < 100; i++)
            {
                sut.Request(GenerateAddress(i), $"MacAddress{i}", TimeSpan.FromMilliseconds(100));
            }

            var available2 = sut.Available;
            Console.WriteLine($"available2 = {available2}");

            Assert.AreNotEqual(available1, available2);
            Thread.Sleep(1000);

            sut.Evict();

            Console.WriteLine($"available3 = {sut.Available}");
            Assert.AreEqual(available1, sut.Available);
        }

        [TestMethod]
        public void Get_returns_IPAddress()
        {
            var sut = new IPAddressPool(GenerateAddress(1));
            var expect = GenerateAddress(2);

            var actual = sut.Get();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expect, actual);
        }

        [TestMethod]
        public void Get_returns_null()
        {
            var sut = new IPAddressPool(GenerateAddress(1));

            for (byte i = 1; i < 255; i++)
            {
                sut.Request(GenerateAddress(i), $"MacAddress{i}", TimeSpan.FromMinutes(30));
            }

            var actual = sut.Get();

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void Release_removes_lease()
        {
            var sut = new IPAddressPool(GenerateAddress(1));

            var firstAddress = sut.Get();
            Assert.IsNotNull(firstAddress);
            sut.Request(firstAddress, "MacAddress1", TimeSpan.FromMinutes(30));

            var secondAddress = sut.Get();
            Assert.IsNotNull(secondAddress);
            sut.Request(secondAddress, "MacAddress2", TimeSpan.FromMinutes(30));

            sut.Release(firstAddress);
            var thirdAddress = sut.Get();

            Assert.AreNotEqual(firstAddress, secondAddress, "firstAddress != secondAddress");
            Assert.AreEqual(firstAddress, thirdAddress, "firstAddress == thirdAddress");
        }

        [TestMethod]
        public void Renew_returns_false()
        {
            var sut = new IPAddressPool(GenerateAddress(1));
            var clientAddress = GenerateAddress(2);
            var hardwareAddress = "MacAddress1";

            Assert.IsFalse(sut.Renew(clientAddress, hardwareAddress), "Client address is not leased");

            sut.Request(clientAddress, hardwareAddress, TimeSpan.FromMinutes(30));

            Assert.IsFalse(sut.Renew(clientAddress, "DifferentHardwareAddress"), "Hardware address is different");
        }

        [TestMethod]
        public void Renew_returns_true()
        {
            var sut = new IPAddressPool(GenerateAddress(1));
            var clientAddress = GenerateAddress(2);
            var hardwareAddress = "MacAddress1";

            sut.Request(clientAddress, hardwareAddress, TimeSpan.FromMinutes(30));

            Assert.IsTrue(sut.Renew(clientAddress, hardwareAddress));
        }

        [TestMethod]
        public void Request_returns_false()
        {
            var sut = new IPAddressPool(GenerateAddress(1));
            var clientAddress = GenerateAddress(2);
            var hardwareAddress = "MacAddress1";

            Assert.IsTrue(sut.Request(clientAddress, hardwareAddress, TimeSpan.FromMinutes(30)), "Address requested");
            Assert.IsFalse(sut.Request(clientAddress, "DifferentHardwareAddress", TimeSpan.FromMinutes(30)), "Hardware address is different");
        }

        [TestMethod]
        public void Request_returns_true()
        {
            var sut = new IPAddressPool(GenerateAddress(1));

            Assert.IsTrue(sut.Request(GenerateAddress(2), "MacAddress1", TimeSpan.FromMinutes(30)));
        }

        [TestMethod]
        public void TryGet_returns_false()
        {
            var sut = new IPAddressPool(GenerateAddress(1));

            for (byte i = 1; i < 255; i++)
            {
                sut.Request(GenerateAddress(i), $"MacAddress{i}", TimeSpan.FromMinutes(30));
            }

            Assert.IsFalse(sut.TryGet(out var actual));
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void TryGet_returns_true()
        {
            var sut = new IPAddressPool(GenerateAddress(1));
            var expect = GenerateAddress(2);

            Assert.IsTrue(sut.TryGet(out var actual));
            Assert.IsNotNull(actual);
            Assert.AreEqual(expect, actual);
        }
    }
}
