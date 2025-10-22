using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Rule34
{
    public class Application
    {
        static void Main(string[] args)
        {
            Rule34.Atnik.Rule34Controller.Init();
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}