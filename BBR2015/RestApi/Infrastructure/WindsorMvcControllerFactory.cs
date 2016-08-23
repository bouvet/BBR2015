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
            return (IController)_container.Resolve(controllerType);
        }

        public override void ReleaseController(IController controller)
        {
            _container.Release(controller);
            base.ReleaseController(controller);
        }
    }
}


