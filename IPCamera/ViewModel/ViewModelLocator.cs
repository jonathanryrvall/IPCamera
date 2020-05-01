using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using IPCamera.Model;
using IPCamera.Model.Config;
using System;


namespace IPCamera.ViewModel
{
    class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);


            SimpleIoc.Default.Register<MainVM>();
        }

        public MainVM MainVM => SimpleIoc.Default.GetInstance<MainVM>();



    }
}