﻿namespace LandingPage.Utils
{
    public class TemplateInformation<T>
    {
        public string BaseAddress { get; set; }
        public string ARMTemplate { get; set; }
        public string UIDefinitions { get; set; }

        public T Parametrization { get; set; }
    }
}
