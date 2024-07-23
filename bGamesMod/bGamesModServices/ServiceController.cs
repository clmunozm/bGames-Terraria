using System;
using System.Collections.Generic;

namespace bGamesMod.bGamesModServices
{
	internal class ServiceController
	{
		public delegate void ServiceEventHandler(bGamesModService service);

		public event ServiceEventHandler ServiceAdded;

		public event ServiceEventHandler ServiceRemoved;

		private List<bGamesModService> _services;

		/// <summary>
		/// HEROsMod Services laoded into the controller
		/// </summary>
		public List<bGamesModService> Services
		{
			get { return _services; }
		}

		public ServiceController()
		{
			_services = new List<bGamesModService>();
			//HEROsModNetwork.LoginService.MyGroupChanged += LoginService_MyGroupChanged;
		}

		/// <summary>
		/// Add a HEROsModService to the ServiceController
		/// </summary>
		/// <param name="service">Service to add</param>
		public void AddService(bGamesModService service)
		{
			_services.Add(service);
			ServiceAdded?.Invoke(service);
		}

		/// <summary>
		/// Remove a HEROsModService from the ServiceController
		/// </summary>
		/// <param name="service">Service to Remove</param>
		public void RemoveService(bGamesModService service)
		{
			service.Destroy();
			_services.Remove(service);
			ServiceRemoved?.Invoke(service);
		}

		/// <summary>
		/// Remove all HEROsModServices from the ServiceController
		/// </summary>
		public void RemoveAllServices()
		{
			while (_services.Count > 0)
			{
				RemoveService(_services[0]);
			}
		}

		internal void ServiceRemovedCall()
		{
			ServiceRemoved(null);
		}
	}
}