using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestApi.Infrastructure
{
    public class ServiceLocator
    {      
        public static IWindsorContainer Current { get; set; }        
    }
}