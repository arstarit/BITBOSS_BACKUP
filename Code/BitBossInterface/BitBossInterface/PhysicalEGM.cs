using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SASComms;

namespace BitbossInterface
{
    /// <summary>
    /// La physical EGM. La misma tiene varios componentents 
    /// The phisical EGM. Has several components
    /// </summary>
    public class PhysicalEGM
    {
        // La EGMAccounting // The EGMAccounting
        private EGMAccounting _EGMAccounting;
        // El Status de la EGM // The EGMStatus
        public EGMStatus _EGMStatus;
        // LAs settings de la EGM // The EGMSettings
        private EGMSettings _EGMSettings;
        // La info de la EGM // The EGMInfo
        private EGMInfo _EGMInfo;

        private string Status = "";


        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /***********************************************  PRIVATE METHODS **************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/

        #region PRIVATE METHODS
        /// <summary>
        /// Dado un array (o lista) de array de bytes, la idea es "aplanar" la lista en un sólo array que concatene cada elemento
        /// Given an array (or list) of byte arrays, the idea is to "flatten" the list into a single array that concatenates each element
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        private Byte[] join(params Byte[][] arr)
        {
            Byte[] b = new Byte[] { };
            foreach (Byte[] b_ in arr)
            {
                b = b.ToList().Concat(b_.ToList()).ToArray();
            }
            return b;
        }
        #endregion

        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /***********************************************  VALIDATION *******************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/

        #region VALIDATION
        // 
        /// <summary>
        /// Setea la validation type de la PhysicalEGM, dependiendo de lo que venga del parámetro setea System o Enhanced y persiste
        /// Set the validation type of the PhysicalEGM, depending on what comes from the parameter sets System or Enhanced and persists
        /// </summary>
        /// <param name="validationType"></param>
        public void SetValidationType(int validationType)
        {
            if (validationType == EGMValidationType.System.GetHashCode())
                _EGMSettings.ValidationType = EGMValidationType.System;
            if (validationType == EGMValidationType.Enhanced.GetHashCode())
                _EGMSettings.ValidationType = EGMValidationType.Enhanced;
            SaveEGMSettings();

        }

        /// <summary>
        /// Setea la validation extensions de la VirtualEGM
        /// Set the validation extensions of the VirtualEGM
        /// </summary>
        /// <param name="b"></param>
        public void SetValidationExtensions(bool b)
        {
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();

            _EGMSettings.ValidationExtensions = b;
            xmlfile_serializer.SaveXml<EGMSettings>(_EGMSettings, "PhysicalEGMSettings.xml");
        }
        #endregion



        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /***********************************************  PHYSICAL EGM CONTROL *********************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/

        #region PHYSICAL EGM CONTROL

        /// <summary>
        /// Básicamente instancia todos las estructuras principales en esta PhysicalEGM como es EGMAccounting, EGMSettings, EGMStatus y EGMInfo que son persistidas
        /// Basically instantiates all the main structures in this PhysicalEGM as is EGMAccounting, EGMSettings, EGMStatus and EGMInfo that are persisted
        /// </summary>
        public void StartPhysicalEGM()
        {

        }

        /// <summary>
        /// Rutina de inicialización
        /// Initialization routine
        /// </summary>
        public PhysicalEGM()
        {
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            // La EGMAccounting, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMAccounting, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMAccounting = xmlfile_serializer.Deserialize<EGMAccounting>("PhysicalEGMAccounting.xml");
                _EGMAccounting.SetGames(true);
            }
            catch
            {
                _EGMAccounting = new EGMAccounting();
                _EGMAccounting.SetGames(true);
                xmlfile_serializer.SaveXml<EGMAccounting>(_EGMAccounting, "PhysicalEGMAccounting.xml");
            }
            // La EGMSettings, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMSettings, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMSettings = xmlfile_serializer.Deserialize<EGMSettings>("PhysicalEGMSettings.xml");
            }
            catch
            {
                _EGMSettings = new EGMSettings();
                xmlfile_serializer.SaveXml<EGMSettings>(_EGMSettings, "PhysicalEGMSettings.xml");
            }
            // La EGMStatus, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMStatus, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMStatus = xmlfile_serializer.Deserialize<EGMStatus>("PhysicalEGMStatus.xml");
            }
            catch
            {
                _EGMStatus = new EGMStatus();
                xmlfile_serializer.SaveXml<EGMStatus>(_EGMStatus, "PhysicalEGMStatus.xml");
            }
            // La EGMInfo, saca los datos persistidos. Si no existe, instancia una nueva y la persiste // The EGMInfo, takes the persisted data. If it does not exist, it instantiates a new one and persists it
            try
            {
                _EGMInfo = xmlfile_serializer.Deserialize<EGMInfo>("PhysicalEGMInfo.xml");
            }
            catch
            {
                _EGMInfo = new EGMInfo();
                xmlfile_serializer.SaveXml<EGMInfo>(_EGMInfo, "PhysicalEGMInfo.xml");
            }
        }

        #endregion

        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /***********************************************  METERS ***********************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/
        /*******************************************************************************************************************/

        #region METERS

        #region "Updating Meters"
        public void UpdateGamesPlayed(byte[] gameNumber, int value)
        {
            // Call the UpdateMeter method of the _EGMAccounting object to update the games played meter.
            // The gameNumber parameter represents the game identifier, and 0x05 represents the meter code for games played.
            // The value parameter contains the value to update the games played meter.
            _EGMAccounting.UpdateMeter(gameNumber, 0x05, value);
        }
        public void UpdateGamesWon(byte[] gameNumber, int value)
        {
            // Call the UpdateMeter method of the _EGMAccounting object to update the games won meter.
            // The gameNumber parameter represents the game identifier, and 0x06 represents the meter code for games won.
            // The value parameter contains the value to update the games won meter.
            _EGMAccounting.UpdateMeter(gameNumber, 0x06, value);
        }
        public void UpdatePowerReset(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, "PowerReset", value);

        }
        public void UpdateSlootDoorOpen(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, "SlotDoorOpen", value);

        }
        public void UpdateTotalCoinIn(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x00, value);

        }
        public void UpdateTotalCoinOut(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x01, value);

        }
        public void UpdateTotalDrop(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, "TotalDrop", value);

        }
        public void UpdateTotalJackPot(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x02, value);

        }
        public void Update000C(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x0C, value);

        }
        public void Update001B(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x1B, value);

        }

        public void Update0004(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x04, value);

        }

        public void Update0015(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x15, value);

        }

        public void Update0016(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x16, value);

        }

        public void Update0017(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x17, value);

        }

        public void Update0018(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x18, value);

        }
        public void Update0024(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x24, value);

        }
        public void Update0028(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x28, value);

        }
        public void Update0029(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x29, value);

        }
        public void Update002A(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x2A, value);

        }
        public void Update002B(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x2B, value);

        }

        public void Update002C(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x2C, value);

        }

        public void Update002D(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x2D, value);

        }
        public void Update002E(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x2E, value);
        }
        public void Update0032(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x32, value);
        }
        public void Update0035(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x35, value);

        }
        public void Update0036(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x36, value);

        }
        public void Update0037(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x37, value);

        }
        public void Update0038(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x38, value);

        }
        public void Update0039(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x39, value);

        }
        public void Update0080(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x80, value);

        }
        public void Update0081(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x81, value);

        }
        public void Update0082(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x82, value);

        }
        public void Update0083(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x83, value);

        }
        public void Update0084(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x84, value);

        }
        public void Update0085(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x85, value);

        }
        public void Update0086(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x86, value);

        }
        public void Update0087(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x87, value);

        }
        public void Update0088(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x88, value);

        }
        public void Update0089(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x89, value);

        }
        public void Update008A(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x8A, value);

        }
        public void Update008B(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x8B, value);

        }
        public void Update008C(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x8C, value);

        }
        public void Update008D(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x8D, value);

        }
        public void Update008E(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x8E, value);

        }
        public void Update008F(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x8F, value);

        }
        public void Update0090(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x90, value);

        }
        public void Update0091(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x91, value);

        }
        public void Update0092(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x92, value);

        }
        public void Update0093(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x93, value);

        }

        public void Update003E(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x3E, value);

        }

        public void Update003F(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x3F, value);

        }
        public void Update0040(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x40, value);

        }
        public void Update0041(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x41, value);

        }
        public void Update0042(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x42, value);

        }
        public void Update0043(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x43, value);

        }
        public void Update0044(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x44, value);

        }
        public void Update0045(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x45, value);

        }
        public void Update0046(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x46, value);

        }
        public void Update0047(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x47, value);

        }
        public void Update0048(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x48, value);

        }
        public void Update0049(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x49, value);

        }
        public void Update0050(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x50, value);

        }
        public void Update0051(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x51, value);

        }
        public void Update0052(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x52, value);

        }
        public void Update0053(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x53, value);

        }
        public void Update0054(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x54, value);

        }
        public void Update0055(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x55, value);

        }
        public void Update0056(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x56, value);

        }
        public void Update0057(byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, 0x57, value);

        }



        /* ACTUALIZACIÓN GENÉRICA DE METERS */
        /* GENERIC UPDATE OF METERS */

        /// <summary>
        /// Actualización de meters variante 1. Cuando el código es un byte, de la lista de meter codes del protocolo. Dichos valores los guarda en EGMAccounting
        /// Update meters 1. When the code is a byte, from the meter codes list of the protocol. These values are saved in EGMAccounting
        /// </summary>
        /// <param name="code"></param>
        /// <param name="gameNumber"></param>
        /// <param name="value"></param>
        public void UpdateMeter(byte code, byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, code, value);
        }

        /// <summary>
        /// Actualización de meters variante 2. Cuando el código es una string, cuyo valor o meter // Update meters 2. When the code is a string, whose value or meter
        /// no está en la lista de metercodes del protocolo. Dichos valores los guarda en EGMAccounting // is not in the metercodes list of the protocol. These values are saved in EGMAccounting
        /// </summary>
        /// <param name="code"></param>
        /// <param name="gameNumber"></param>
        /// <param name="value"></param>
        public void UpdateMeter(string code, byte[] gameNumber, int value)
        {
            _EGMAccounting.UpdateMeter(gameNumber, code, value);
        }

        /// <summary>
        /// Método que permite persistir la EGM Accounting cuando todos los meters actualizaron // Method that allows to persist the EGM Accounting when all the meters updated
        /// </summary>
        public void AllMetersUpdated()
        {
            SaveEGMAccounting();
        }

        #endregion

        #region "GetMeters"
        public int GetGamesPlayed()
        {
            // Call the GetGameAccounting method of the _EGMAccounting object with the specified game number.
            // Then access the Value property of the Meter_Basic_GamesPlayed property to get the value of games played.
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_GamesPlayed.Value;
        }

        public int GetGamesWon()
        {
            // Call the GetGameAccounting method of the _EGMAccounting object with the specified game number.
            // Then access the Value property of the Meter_Basic_GamesWon property to get the value of games won.
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_GamesWon.Value;
        }

        public int GetPowerReset()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_PowerReset.Value;

        }
        public int GetSlootDoorOpen()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_SlotDoorOpen.Value;

        }
        public int GetTotalCoinIn()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_TotalCoinIn.Value;

        }
        public int GetTotalCoinOut()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_TotalCoinOut.Value;

        }
        public int GetTotalDrop()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_TotalDrop.Value;

        }
        public int GetTotalJackPot()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_Basic_TotalJackPot.Value;

        }

        public int Get000C()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_000C.Value;

        }
        public int Get001B()
        {
            return _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }).Meter_001B.Value;

        }

        /* GET Genérico de meters */

        /// <summary>
        /// Get genérico de meters, consulta en EGMAccounting, todo lo correspondiente al game 0 // Generic Get of meters, query in EGMAccounting, everything corresponding to game 0
        /// </summary>
        /// <param name="code"> El metercode </param>
        /// <returns> Retorna un entero, correspondiente al valor del meter </returns> // Returns an integer, corresponding to the value of the meter
        public int GetMeter(byte code)
        {
            return int.Parse(BitConverter.ToString(
                _EGMAccounting.GetValueOfMeter(new byte[] { code, 0x00 },
                _EGMAccounting.GetGameAccounting(new byte[] { 0x00, 0x00 }), false))
                .Replace("-", ""));
        }


        #endregion

        #endregion




        /****************************************************************************************************************************/
        /****************************************************************************************************************************/
        /**********************************************      METHODS FOR PHYSICALEGM    **********************************************/
        /****************************************************************************************************************************/
        /****************************************************************************************************************************/

        #region METHODS FOR PHYSICALEGM

        #region "Redemption"

        #endregion

        #region "Game Machine"

        /// <summary>
        /// Setea las features de la PhysicalEGM. Seteo desde la EGM // Set the features of the PhysicalEGM. Set from the EGM
        /// </summary>
        /// <param name="features1"></param>
        /// <param name="features2"></param>
        /// <param name="features3"></param>
        public void SetFeatures(byte features1, byte features2, byte features3)
        {
            _EGMSettings.features1 = features1;
            _EGMSettings.features2 = features2;
            _EGMSettings.features3 = features3;

            SaveEGMSettings();
        }


        /// <summary>
        /// Actualiza el límite de cashout en la setting del game // Update the cashout limit in the game setting
        /// provisto por parámetro // provided by parameter
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="cashoutLimit"></param>
        public void UpdateCashoutLimit(byte[] gameNumber, byte[] cashoutLimit)
        {
            _EGMSettings.GetGameSettings(gameNumber).CashoutLimit = cashoutLimit;
            SaveEGMSettings();
        }


        /// <summary>
        /// Método que actualiza toda la información del juego en EGM Info // Method that updates all the game information in EGM Info
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="additionalId"></param>
        /// <param name="denomination"></param>
        /// <param name="maxBet"></param>
        /// <param name="progressiveGroup"></param>
        /// <param name="gameOptions"></param>
        /// <param name="paytableId"></param>
        /// <param name="basePercentage"></param>
        public void UpdateGamingInfo_GameID(string gameId,
                                            string additionalId,
                                            byte denomination,
                                            byte maxBet,
                                            byte progressiveGroup,
                                            byte[] gameOptions,
                                            string paytableId,
                                            string basePercentage)
        {
            _EGMInfo.GameID = gameId;
            _EGMInfo.AdditionalID = additionalId;
            _EGMInfo.BasePercentage = basePercentage;
            _EGMInfo.Denomination = denomination;
            _EGMSettings.GameOptions = gameOptions;
            _EGMSettings.MaxBet = maxBet;
            _EGMInfo.PayTableID = paytableId;
            _EGMSettings.ProgressiveGroup = progressiveGroup;
            SaveEGMInfo();
        }


        /// <summary>
        /// Método que actualiza toda la información del juego a nivel game en EGM Info y EGM Settings // Method that updates all the game information at game level in EGM Info and EGM Settings
        /// </summary>
        public void UpdateGamingNInfo(byte[] gameNumber,
                                      byte[] maxBet,
                                      byte progressiveGroup,
                                      byte[] progressiveLevels,
                                      byte gameNameLength,
                                      byte[] gameName,
                                      byte paytableLength,
                                      byte[] paytableName,
                                      byte[] wagerCategories)
        {
            GameInfo gameInfo = _EGMInfo.GetGameInfo(gameNumber);
            GameSettings gameSettings = _EGMSettings.GetGameSettings(gameNumber);

            gameInfo.gameName = System.Text.Encoding.ASCII.GetString(gameName);
            gameInfo.paytableName = System.Text.Encoding.ASCII.GetString(paytableName);
            gameInfo.wagerCategories = wagerCategories;

            SaveEGMInfo();

            gameSettings.maxBet = maxBet;
            gameSettings.progressiveGroup = progressiveGroup;
            gameSettings.progressiveLevels = progressiveLevels;

            SaveEGMSettings();
        }


        /// <summary>
        /// Método que setea la fecha y hora de la EGMStatus en formato unix // Method that sets the date and time of the EGMStatus in unix format
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        public void SetDateAndTime(int day, int month, int year, int hour, int minute, int second)
        {
            _EGMStatus.EGMLastPolledTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            _EGMStatus.EGMTime = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero).ToUnixTimeSeconds();
            SaveEGMStatus();

        }

        /// <summary>
        /// Método que setea la denominación del jugador actual, en EGM Status // Method that sets the current player's denomination, in EGM Status
        /// </summary>
        /// <param name="currentDenomination"></param>
        public void SetCurrentPlayerDenomination(byte currentDenomination)
        {
            _EGMStatus.CurrentPlayerDenomination = currentDenomination;
            SaveEGMStatus();

        }

        /// <summary>
        ///  Método que actualiza el número de denominaciones y las denominaciones del jugador en la EGM Status // Method that updates the number of denominations and the player's denominations in the EGM Status
        /// </summary>
        /// <param name="NumberOfDenominations">Número de denominaciones</param>
        /// <param name="PlayerDenominations">Denominaciones del jugador</param>
        public void SetDenominations(byte NumberOfDenominations, byte[] PlayerDenominations)
        {
            _EGMStatus.NumberOfDenominations = NumberOfDenominations;
            _EGMStatus.PlayerDenominations = PlayerDenominations;
            SaveEGMStatus();
        }

        /// <summary>
        /// Método que actualiza el TokenDenomination en la EGMStatus // Method that updates the TokenDenomination in the EGMStatus
        /// </summary>
        /// <param name="TokenDenomination"></param>
        public void SetTokenDenomination(byte TokenDenomination)
        {
            _EGMStatus.TokenDenomination = TokenDenomination;
            SaveEGMStatus();
        }

        /// <summary>
        /// Obtiene la versionID de EGM Info // Get the VersionID from EGMInfo
        /// Get the VersionID from EGMInfo // Get the VersionID from EGMInfo
        /// </summary>
        public byte[] GetVersionID()
        {
            return _EGMInfo.SASVersion;
        }

        /// <summary>
        /// Método que setea la versionID en EGM Info // Method that sets the versionID in EGMInfo
        /// </summary>
        /// <param name="versionID"></param>
        public void SetVersionID(byte[] versionID)
        {
            _EGMInfo.SASVersion = versionID;
            SaveEGMSettings();
        }

        /// <summary>
        /// Obtiene la versionID de EGM Settings // Get the VersionID from EGMSettings
        /// Get the Serial Number from EGMSettings // Get the Serial Number from EGMSettings
        /// </summary>
        public byte[] GetGameMachineSerialNumber()
        {
            return _EGMInfo.GMSerialNumber;
        }


        /// <summary>
        /// Método que setea el número serial de la GameMachine en EGM Settings // Method that sets the serial number of the GameMachine in EGMSettings
        /// </summary>
        /// <param name="gmSerialNumber"></param>
        public void SetGameMachineSerialNumber(byte[] gmSerialNumber)
        {
            _EGMInfo.GMSerialNumber = gmSerialNumber;
            SaveEGMSettings();
        }

        /// <summary>
        /// Se obtiene la EGM Info de la PhysicalEGM // Get the EGM Info from the PhysicalEGM
        /// </summary>
        /// <returns></returns>
        public EGMInfo GetEGMInfo()
        {
            return _EGMInfo;
        }

        /// <summary>
        /// Se obtiene la EGM Settings de la PhysicalEGM // Get the EGM Settings from the PhysicalEGM
        /// </summary>
        /// <returns></returns>
        public EGMSettings GetEGMSettings()
        {
            return _EGMSettings;
        }

        // RAM Clear
        public void RAMClear()
        {
            _EGMAccounting.ResetMeters();
        }

        #endregion

        #region "Games"

        /// <summary>
        ///  Método que permite actualizar el campo NumberOfGamesImplemented de la EGM Info. // Method that allows updating the NumberOfGamesImplemented field of the EGM Info.
        /// </summary>
        /// <param name="numberOfGames"></param>
        public void SetNumberOfGames(byte[] numberOfGames)
        {
            _EGMInfo.NumberOfGamesImplemented = int.Parse(BitConverter.ToString(numberOfGames).Replace("-", ""));
            SaveEGMInfo();
        }

        /// <summary>
        ///  Obtener el selected game de la EGM Status. Devuelve el gameNmber correspondiente al current selected game. // Get the selected game from the EGM Status. Returns the gameNmber corresponding to the current selected game.
        /// </summary>
        /// <returns> </returns>
        public byte[] GetSelectedGame()
        {
            if (_EGMStatus.CurrentGameNumber == null)
                return new byte[] { 0x00, 0x00 };
            return _EGMStatus.CurrentGameNumber;
        }

        /// <summary>
        ///  Método que permite actualizar el campo CurrentGameNumber de la EGM Status // Method that allows updating the CurrentGameNumber field of the EGM Status
        /// </summary>
        /// <param name="gameNumber"></param>
        public void SetCurrentGameNumber(byte[] gameNumber)
        {
            _EGMStatus.CurrentGameNumber = gameNumber;
            SaveEGMStatus();

        }

        /// <summary>
        ///  Método que permite actualizar el campo EnabledGameNumbers // Method that allows updating the EnabledGameNumbers field
        /// </summary>
        /// <param name="EnabledGames"></param>
        public void SetEnabledGameNumbers(List<byte[]> EnabledGames)
        {
            _EGMSettings.EnabledGameNumbers = EnabledGames;
            SaveEGMSettings();

        }

        /// <summary> 
        ///  Método que permite actualizar la information del último billete aceptado // Method that allows updating the information of the last accepted ticket
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="denominationCode"></param>
        /// <param name="billMeter"></param>
        public void SetLastAcceptedBillInformation(byte countryCode,
                                                   byte denominationCode,
                                                   byte[] billMeter)
        {
            _EGMStatus.LastBillInformation = join(new byte[] { countryCode, denominationCode },
                                                  billMeter);
            SaveEGMStatus();

        }

        /// <summary>
        /// Seteo de la extension validation status // Extension validation status setting
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="denominationCode"></param>
        /// <param name="billMeter"></param>
        /// <param name="restrictedTicketDefaultExpiration"></param>
        public void SetExtendedValidationStatus(byte[] assetNumber,
                                                byte[] statusBits,
                                                byte[] cashableTicketAndReceiptExpiration,
                                                byte[] restrictedTicketDefaultExpiration)
        {
            // Asset Number
            _EGMSettings.assetId = assetNumber;
            // Los estados. // The states.
            // ASUMO: Por cada bit de estado, si la máscara de control me habilita a actualizar el valor // I assume: For each state bit, if the control mask allows me to update the value
            // Habilito o deshabilito la función a travès de lo que me venga como parámetro en statusBitControlStates // I enable or disable the function through what comes to me as a parameter in statusBitControlStates
            _EGMSettings.statusBitControlStates = statusBits;
            // Expiración del ticket y el receipt // Ticket and receipt expiration
            _EGMSettings.cashableTicketAndReceiptExpiration = cashableTicketAndReceiptExpiration;
            // Expiración del default ticket // Default ticket expiration
            _EGMSettings.restrictedTicketDefaultExpiration = restrictedTicketDefaultExpiration;

            SaveEGMSettings();

        }

        /// <summary>
        /// Set Last EGM Response timestamp to EGMStatus // Set Last EGM Response timestamp to EGMStatus
        /// </summary>
        /// <param name="ts"></param>
        public void SetLastEGMResponseTS(DateTime ts)
        {
            // Last EGM response time
            _EGMStatus.LastEGMResponseReceivedAt = ts;
            SaveEGMStatus();

        }

        /// <summary>
        /// GET Last EGM Response timestamp from EGMStatus // GET Last EGM Response timestamp from EGMStatus
        /// </summary>
        /// <param name="ts"></param>
        public DateTime GetLastEGMResponseTS()
        {
            return _EGMStatus.LastEGMResponseReceivedAt;

        }


        #endregion


        /// <summary>
        /// ** EGM Settings and EGM Info ** *** AFT ***
        /// 74 Response handler from VirtualEGMController (at MainController).
        /// When a long poll 74 arrives to Host and processed by MainController, the vegm controller calls this method  to persist its parameters to EGMSettings and EGMStatus
        /// </summary>
        /// <param name="assetNumber">Into EGMSettings</param>
        /// <param name="availableTransfers">Into EGMSettings with specific mask</param>
        /// <param name="gameLockStatus">Into EGMStatus</param>
        /// <param name="hostCashoutStatus">Into EGMStatus</param>
        /// <param name="aftStatus">Into EGMStatus with specific mask</param>
        /// <param name="restrictedExpiration">Into EGMStatus</param>
        /// <param name="gmTransferLimit">Into EGMSettings</param>
        /// <param name="gmMaxBufferIndex">Into EGMSettings</param>
        /// <param name="currentRestrictedAmount">Into EGMStatus</param>
        /// <param name="currentNonRestrictedAmount">Into EGMStatus</param>
        /// <param name="restrictedPoolID">Into EGMSettings</param>
        public void Update74ResponseInfo(byte[] assetNumber, byte availableTransfers, byte gameLockStatus, byte hostCashoutStatus, byte aftStatus, byte[] restrictedExpiration, byte[] gmTransferLimit, byte gmMaxBufferIndex, byte[] currentCashableAmount, byte[] currentRestrictedAmount, byte[] currentNonRestrictedAmount, byte[] restrictedPoolID)
        {
            bool saveegmsettings = false;
            bool saveegmstatus = false;

            /** AssetNumber **/
            if (!_EGMSettings.assetId.SequenceEqual(assetNumber))
            {
                // Asset Number
                _EGMSettings.assetId = assetNumber;
                saveegmsettings = true;
            }

            /** Available Transfers **/
            if (_EGMStatus.availableTransfers != availableTransfers)
            {
                // Available Transfers
                _EGMStatus.availableTransfers = availableTransfers;
                saveegmstatus = true;
            }

            /** Game Lock Status **/

            if (_EGMStatus.gameLockStatus != gameLockStatus)
            {
                //  Game Lock Status
                _EGMStatus.gameLockStatus = gameLockStatus;
                saveegmstatus = true;
            }

            /** Host Cashout Status **/

            if (_EGMStatus.hostCashoutStatus != hostCashoutStatus)
            {
                // Host Cashout Status
                _EGMStatus.hostCashoutStatus = hostCashoutStatus;
                saveegmstatus = true;
            }


            /** AFT Status **/
            if (_EGMStatus.aftStatus != aftStatus)
            {
                // AFT Status
                _EGMStatus.aftStatus = aftStatus;
                saveegmstatus = true;
            }

            /** Restricted Expiration **/

            if (!_EGMStatus.restrictedExpiration.SequenceEqual(restrictedExpiration))
            {
                // Restricted Expiration
                _EGMStatus.restrictedExpiration = restrictedExpiration;
                saveegmstatus = true;
            }

            /** Game Machine Transfer Limit **/
            if (!_EGMSettings.gmTransferLimit.SequenceEqual(gmTransferLimit))
            {
                // Game Machine Transfer Limit
                _EGMSettings.gmTransferLimit = gmTransferLimit;
                saveegmsettings = true;
            }

            /** Game Machine Buffer Index **/
            if (_EGMSettings.gmMaxBufferIndex != gmMaxBufferIndex)
            {
                // Game Machine Buffer Index
                _EGMSettings.gmMaxBufferIndex = gmMaxBufferIndex;
                _EGMStatus.aftcollection.maxBufferSize = gmMaxBufferIndex;
                saveegmsettings = true;
            }

            /** Current Cashable Amount **/
            if (!_EGMStatus.currentCashableAmount.SequenceEqual(currentCashableAmount))
            {
                // Cashable Amount
                _EGMStatus.currentCashableAmount = currentCashableAmount;
                saveegmstatus = true;
            }

            /** Current Restricted Amount **/

            if (!_EGMStatus.currentRestrictedAmount.SequenceEqual(currentRestrictedAmount))
            {
                // Restricted Expiration
                _EGMStatus.currentRestrictedAmount = currentRestrictedAmount;
                saveegmstatus = true;
            }

            /** Current Non Restricted Amount **/

            if (!_EGMStatus.currentNonRestrictedAmount.SequenceEqual(currentNonRestrictedAmount))
            {
                // Restricted Expiration
                _EGMStatus.currentNonRestrictedAmount = currentNonRestrictedAmount;
                saveegmstatus = true;
            }

            /** Restricted Pool ID **/
            if (!_EGMSettings.restrictedPoolID.SequenceEqual(restrictedPoolID))
            {
                _EGMSettings.restrictedPoolID = restrictedPoolID;
                saveegmsettings = true;
            }



            if (saveegmsettings)
                SaveEGMSettings();

            if (saveegmstatus)
                SaveEGMStatus();
        }
        /// <summary>
        /// Se actualiza la data del ticket // Ticket Data Update
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        public void UpdateTicketData(byte code, string data)
        {
            if (code == 0x00)
                _EGMSettings.LocationName = data;
            if (code == 0x01)
                _EGMSettings.LocationAddress1 = data;
            if (code == 0x02)
                _EGMSettings.LocationAddress2 = data;
            if (code == 0x10)
                _EGMSettings.RestrictedTicketTitle = data;
            if (code == 0x20)
                _EGMSettings.DebitTicketTitle = data;
        }
        #endregion

        // Persisto los cambios no guardados en los xml // Save the changes not saved in the xml

        public void SaveEGMSettings()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMSettings>(_EGMSettings, "PhysicalEGMSettings.xml");
        }

        public void SaveEGMStatus()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();

            xmlfile_serializer.SaveXml<EGMStatus>(_EGMStatus, "PhysicalEGMStatus.xml");
        }

        public void SaveEGMAccounting()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMAccounting>(_EGMAccounting, "PhysicalEGMAccounting.xml");

        }

        public void SaveEGMInfo()
        {
            // El serializer XML para cada una de las estructuras mencionadas anteriormente // The XML serializer for each of the structures mentioned above
            XmlFileSerializer xmlfile_serializer = new XmlFileSerializer();
            xmlfile_serializer.SaveXml<EGMInfo>(_EGMInfo, "PhysicalEGMInfo.xml");

        }

    }
}
