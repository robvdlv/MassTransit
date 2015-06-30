// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConsumeConfigurators;
    using Internals.Extensions;
    using Microsoft.Practices.Unity;
    using Saga;
    using Saga.SubscriptionConfigurators;
    using UnityIntegration;


    public static class UnityExtensions
    {
        public static void LoadFrom(this IReceiveEndpointConfigurator configurator, IUnityContainer container)
        {
            IList<Type> concreteTypes = FindTypes<IConsumer>(container, x => !x.HasInterface<ISaga>());
            if (concreteTypes.Count > 0)
            {
                foreach (Type concreteType in concreteTypes)
                    ConsumerConfiguratorCache.Configure(concreteType, configurator, container);
            }

            IList<Type> sagaTypes = FindTypes<ISaga>(container, x => true);
            if (sagaTypes.Count > 0)
            {
                foreach (Type sagaType in sagaTypes)
                    SagaConfiguratorCache.Configure(sagaType, configurator, container);
            }
        }

        public static void Consumer<T>(this IReceiveEndpointConfigurator configurator, IUnityContainer container,
            Action<IConsumerConfigurator<T>> configure = null)
            where T : class, IConsumer
        {
            var consumerFactory = new UnityConsumerFactory<T>(container);

            configurator.Consumer(consumerFactory, configure);
        }

        public static void Saga<T>(this IReceiveEndpointConfigurator configurator, IUnityContainer container,
            Action<ISagaConfigurator<T>> configure = null)
            where T : class, ISaga
        {
            var sagaRepository = container.Resolve<ISagaRepository<T>>();

            var unitySagaRepository = new UnitySagaRepository<T>(sagaRepository, container);

            configurator.Saga(unitySagaRepository, configure);
        }

        static IList<Type> FindTypes<T>(IUnityContainer container, Func<Type, bool> filter)
        {
            return container.Registrations
                .Where(r => r.MappedToType.HasInterface<T>())
                .Select(r => r.MappedToType)
                .Where(filter)
                .ToList();
        }
    }
}