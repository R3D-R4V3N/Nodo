using Rise.Client.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Client.Faker;
internal class FakeHubClientFactory : IHubClientFactory
{
    public IHubClient Create() => new FakeHubClient();
}