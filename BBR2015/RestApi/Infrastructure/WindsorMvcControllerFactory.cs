using System;
using System.Web.Mvc;
using Castle.Windsor;

namespace RestApi.Infrastructure
{
    public class WindsorMvcControllerFactory : DefaultControllerFactory
    {

        private readonly IWindsorContainer _container;

        public WindsorMvcControllerFactory(IWindsorContainer container)
        {
            _container = container;
        }

        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null) return null;
            var resolved = _container.Resolve(controllerType);
            return (IController)resolved;
        }

        public override void ReleaseController(IController controller)
        {
            _container.Release(controller);
            base.ReleaseController(controller);
        }
    }
}


