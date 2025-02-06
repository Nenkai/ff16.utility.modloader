using ff16.utility.modloader.Template.Configuration;

using Reloaded.Mod.Interfaces.Structs;

using System.ComponentModel;
using System.Threading.Channels;

namespace ff16.utility.modloader.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Merge nex/nxd changes")]
        [Description("Whether to merge Nex (.nxd) changes made to a specific nex table by multiple tables.\nNOTE: This should always be enabled.")]
        [DefaultValue(true)]
        public bool MergeNexFileChanges { get; set; } = true;

        [DisplayName("Log nex cell changes")]
        [Description("Whether to log specific row cell changes made to nex tables made by mods.\nNOTE: \"Merge Nex / Nxd Changes\" must be enabled.")]
        [DefaultValue(false)]
        public bool LogNexCellChanges { get; set; } = false;

        [DisplayName("Add mod info to main menu")]
        [Description("Whether to add mod information to the main menu.")]
        [DefaultValue(true)]
        public bool AddMainMenuModInfo { get; set; } = true;

        [DisplayName("Remove exception/crash handler")]
        [Description("(Advanced users only) Whether to remove the default exception handler." +
            "Removes the 'An unexpected error has occurred. Exiting FINAL FANTASY XVI.' message on crash.")]
        [DefaultValue(true)]
        public bool RemoveExceptionHandler { get; set; } = true;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
