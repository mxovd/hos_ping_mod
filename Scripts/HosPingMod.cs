using HarmonyLib;

class HosPingMod : GameModification
{
    Harmony _harmony;

    public HosPingMod(Mod mod) : base(mod)
    {
        Log("Registering ping systems...");
    }

    public override void OnModInitialization(Mod mod)
    {
        Log("Initializing tile ping support...");

        PatchGame();
    }

    public override void OnModUnloaded()
    {
        Log("Unloading tile ping support...");

        _harmony?.UnpatchAll(_harmony.Id);
    }

    void PatchGame()
    {
        Log("Patching tile interactions...");

        _harmony = new Harmony("com.hexofsteel." + Mod.Name);
        _harmony.PatchAll();
    }
}
