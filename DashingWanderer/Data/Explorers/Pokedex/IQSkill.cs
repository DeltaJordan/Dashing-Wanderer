using System;
using System.Collections.Generic;
using System.Text;
using DashingWanderer.Data.Explorers.Pokedex.Enums;

namespace DashingWanderer.Data.Explorers.Pokedex
{
    public class IQSkill
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IQEnum.IQGroup Group { get; set; }
        public string Effect { get; set; }
        public int RequiredIQ { get; set; }
    }
}
