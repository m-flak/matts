using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matts.AzFunctions.Tests;
public class Dummy
{
    [Fact]
    public void DummyTest()
    {
        const bool bTrue = true;
        Assert.True(bTrue);
    }
}
