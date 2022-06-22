#pragma once
#include "MultiClampBroadcastMsg.hpp"

#ifndef MULTICLAMPTELEGRAPH_H
#define MULTICLAMPTELEGRAPH_H

/*********************************
 *     APPLICATION DEFINES       *
 *********************************/
 ///@brief The version number. This should be incremented when this file is changed.
#define MCT_VERSION "0.2"
 ///@brief This string is used in place of fields not supported by the 700A.
#define UNSPECIFIED_FOR_700A "UNSPECIFIED"
///@brief This string is used as the class name for the messaging window.
#define MCT_WINDOWCLASS_NAME "MCT_windowClass"


/*********************************
 *       TYPE DEFINITIONS        *
 *********************************/

 /**
  * @brief A representation of an amplifier/channel state.
  *
  * Each callback is identified by a unique ID, which also serves as its address for communcation purposes.
  */
typedef struct
{
	///Unique identifier for, and address of, this amplifier/channel.
	LPARAM ID;
	/**
	 * @brief Operating mode of the amplifier.
	 *
	 * The available modes are -\n
	 *  @indent V-Clamp\n
	 *  @indent I-Clamp\n
	 *  @indent I = 0\n
	 *
	 * The unsigned integer is interpreted as an offset into the MCTG_MODE_NAMES array of strings.
	 *
	 * @see MCTG_MODE_NAMES
	 */
	unsigned int uOperatingMode;
	/**
	 * @brief Signal identifier of scaled (primary) output.
	 *
	 * 700A Values:\n
	 *   @indent Membrane Potential / Command Current\n
	 *   @indent Membrane Current\n
	 *   @indent Pipette Potential / Membrane Potential\n
	 *   @indent x100 AC Pipette / Membrane Potential\n
	 *   @indent Bath Current\n
	 *   @indent Bath Potential\n
	 * 700B Values:\n
	 *   @indent Membrane Current\n
	 *   @indent Membrane Potential\n
	 *   @indent Pipette Potential\n
	 *   @indent 100x AC Membrane Potential\n
	 *   @indent Command Current\n
	 *   @indent External Command Potential / External Command Current\n
	 *   @indent Auxiliary 1\n
	 *   @indent Auxiliary 2\n
	 */
	unsigned int uScaledOutSignal;
	///@brief Gain of scaled (primary) output.
	double dAlpha;
	///@brief Scale factor of scaled (primary) output.
	double dScaleFactor;
	/**
	 * @brief Scale factor units of scaled (primary) output.
	 *
	 *  Values:\n
	 *    @indent V/V  - MCTG_UNITS_VOLTS_PER_VOLT\n
	 *    @indent V/mV - MCTG_UNITS_VOLTS_PER_MILLIVOLT\n
	 *    @indent V/uV - MCTG_UNITS_VOLTS_PER_MICROVOLT\n
	 *    @indent V/A  - MCTG_UNITS_VOLTS_PER_AMP\n
	 *    @indent V/mA - MCTG_UNITS_VOLTS_PER_MILLIAMP\n
	 *    @indent V/uA - MCTG_UNITS_VOLTS_PER_MICROAMP\n
	 *    @indent V/nA - MCTG_UNITS_VOLTS_PER_NANOAMP\n
	 *    @indent V/pA - MCTG_UNITS_VOLTS_PER_PICOAMP\n
	 *    @indent None - MCTG_UNITS_NONE\n
	 */
	unsigned int uScaleFactorUnits;
	///@brief Lowpass filter cutoff frequency [Hz] of scaled (primary) output.
	double dLPFCutoff;
	///@brief Membrane capacitance [F].
	double dMembraneCap;
	/**
	 * @brief External command sensitivity.
	 *
	 *  Values:\n
	 *    @indent V/V (V-Clamp)\n
	 *    @indent A/V (I-Clamp)\n
	 *    @indent 0.0 A/V, OFF (I=0)\n
	 */
	double dExtCmdSens;
	/**
	 * @brief Signal identifier of raw (secondary) output.
	 *
	 * 700A Values:\n
	 *   @indent Membrane plus Offset Potential / Command Current\n
	 *   @indent Membrane Current\n
	 *   @indent Pipette Potential / Membrane plus Offset Potential\n
	 *   @indent x100 AC Pipette / Membrane Potential\n
	 *   @indent Bath Current\n
	 *   @indent Bath Potential\n
	 * 700B Values:\n
	 *   @indent Membrane Current\n
	 *   @indent Membrane Potential\n
	 *   @indent Pipette Potential\n
	 *   @indent 100x AC Membrane Potential\n
	 *   @indent External Command Potential / External Command Current\n
	 *   @indent Auxiliary 1\n
	 *   @indent Auxiliary 2\n
	 */
	unsigned int uRawOutSignal;
	///@brief Scale factor of raw (secondary) output.
	double dRawScaleFactor;
	/**
	 * @brief Scale factor units of raw (secondary) output.
	 *
	 *  Values:\n
	 *    @indent V/V  - MCTG_UNITS_VOLTS_PER_VOLT\n
	 *    @indent V/mV - MCTG_UNITS_VOLTS_PER_MILLIVOLT\n
	 *    @indent V/uV - MCTG_UNITS_VOLTS_PER_MICROVOLT\n
	 *    @indent V/A  - MCTG_UNITS_VOLTS_PER_AMP\n
	 *    @indent V/mA - MCTG_UNITS_VOLTS_PER_MILLIAMP\n
	 *    @indent V/uA - MCTG_UNITS_VOLTS_PER_MICROAMP\n
	 *    @indent V/nA - MCTG_UNITS_VOLTS_PER_NANOAMP\n
	 *    @indent V/pA - MCTG_UNITS_VOLTS_PER_PICOAMP\n
	 *    @indent None - MCTG_UNITS_NONE\n
	 */
	unsigned int uRawScaleFactorUnits;
	/**
	 * @brief Hardware type identifier.
	 *
	 * Values:\n
	 *  @indent MCTG_HW_TYPE_MC700A\n
	 *  @indent MCTG_HW_TYPE_MC700B\n
	 */
	unsigned int uHardwareType;
	///@brief Gain of raw (secondary) output.
	double dSecondaryAlpha;
	///@brief Lowpass filter cutoff frequency [Hz] of raw (secondary) output.
	double dSecondaryLPFCutoff;
	///@brief Application version of MultiClamp Commander 2.x.
	char* szAppVersion;
	///@brief Firmware version of MultiClamp 700B.
	char* szFirmwareVersion;
	///@brief DSP version of MultiClamp 700B.
	char* szDSPVersion;
	///@brief Serial number of MultiClamp 700B.
	char* szSerialNumber;
	///@brief The last time this structure has been updated, in system ticks.
	LARGE_INTEGER refreshTickCount;
} MCT_state;

static const char* MCT_SCALE_UNITS[] = { "V/V", "V/mV", "V/uV", "V/A", "V/mA", "V/uA", "V/nA", "V/pA", "None" };



struct MC700Bparam {
	int ID;
	int mode;
	double primary_gain;
	double scaleFactor;
	double LPF_cutoff;
	double external_cmd_sensitivity;
	double second_alpha;
	double second_LPF_cutoff;
};


wchar_t *convertCharArrayToLPCWSTR(const char* charArray);
void MCT_init();
void MCT_broadcast();
int MCT_getAmplifierIndex(LPARAM id);
void MCT_shutdown();
int get_MCT_amplifiersLength();
MCT_state* get_MCT_State(int id);
//BOOL MCTG_Unpack700ASignalIDs(LPARAM lparamSignalIDs, UINT *puComPortID, UINT   *puAxoBusID, UINT   *puChannelID)

#endif MULTICLAMPTELEGRAPH_H