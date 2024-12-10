using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class ActivationManager
    {
        // Fields
        private readonly IList<IActivationProvider> _activationProviderList;


        // Constructors
        public ActivationManager(IList<IActivationProvider> activationProviderList)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(activationProviderList);

            #endregion

            // Default
            _activationProviderList = activationProviderList.ToList();
        }


        // Methods
        public bool HandleUrl(string url)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(url);

            #endregion

            // ActivationProviderList
            foreach (var activationProvider in _activationProviderList)
            {
                if (activationProvider.HandleUrl(url) == true)
                {
                    return true;
                }
            }

            // Return
            return false;
        }
    }
}
