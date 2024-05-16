namespace ColorfulFishPonds {
    internal class ModConfig {

        public bool ModEnabled { get; set; } = true;
        public bool DyeColors { get; set; } = true;
        public int RequiredPopulation { get; set; } = 2;
        public bool DisableSingleRecolors { get; set; } = false;
        public bool Debugging { get; set; } = false;
        public float PrismaticSpeed { get; set; } = 0.3f;

    }
}
