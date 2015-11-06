using Castle.Windsor;

namespace Database.Infrastructure
{
    public class ServiceLocator
    {      
        public static IWindsorContainer Current { get; set; }        
    }
}