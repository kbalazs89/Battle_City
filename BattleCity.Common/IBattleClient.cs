using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCity.Common
{
    /// <summary>
    /// Class that represents one battle command
    /// </summary>
    public class BattleCommand
    {
        /// <summary>
        /// Action to perform
        /// </summary>
        public UdpCommand.Action Action { get; set; }

        /// <summary>
        /// Action parameter (for change of speed/rotation)
        /// Speed can be -100 .. 100, you have to use values between 0 .. 200 
        ///     (so 0 corresponds to speed -100 and the value 200 corresponds to speed +100)
        /// Rotation can be 0..360, you have to use values between 0 .. 36
        ///     (so 0 corresponds to 0 degrees (facing to the right side), and 36 corresponds to 360 degrees)
        /// </summary>
        public byte ActionParameter { get; set; }
    }

    /// <summary>
    /// Interface - must use this as a basis
    /// </summary>
    public interface IBattleClient
    {
        /// <summary>
        /// The team name
        /// </summary>
        string TeamName { get; }

        /// <summary>
        /// Image filename in the Images directory
        /// </summary>
        string ImageFile { get; }

        /// <summary>
        /// This method will be called when the level is reset
        /// </summary>
        void GameWasReset();

        /// <summary>
        /// This will be called when the game quits - must close all windows/threads/handles
        /// </summary>
        void ShutDown();

        /// <summary>
        /// This will be called periodically
        /// </summary>
        /// <param name="items">COPY of the gameitem list</param>
        /// <param name="remainingSeconds">Remaining seconds in the current map</param>
        /// <param name="playerId">ID number of the current player</param>
        /// <returns>The command that the AI wants to execute</returns>
        BattleCommand GameTick(IEnumerable<GameItem> items, byte remainingSeconds, byte playerId);
    }
}
