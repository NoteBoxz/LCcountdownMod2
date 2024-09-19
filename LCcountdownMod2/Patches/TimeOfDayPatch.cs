using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LCcountdownMod2
{
	[HarmonyPatch(typeof(TimeOfDay))]
	internal class TimeOfDayUpdatePatch
	{
		private static bool CountingDown;

		private static bool CalcuatedWhenToCountdown;

		private static float DownTimer;

		private static int Number;

		private static SelectableLevel PatchcurrentLevel;

		private static InputAction keyAction = new InputAction(null, InputActionType.Button, "<Keyboard>/5");

		private static float DebugTimer;

		private static float ShipLeavePerdictionNormalized;

		private static float ShipLeavePerdiction;

		private static float PatchGlobalTime;

		private static float PatchTotalTime;

		private static float EarlyCountDownTime;

		private static float EarlyCountDownDuration = 46.5f;

		private static int PlayersInMatch;

		private static bool DisableDisabler = false;

		private static bool LeavingEarly;

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		private static void CountDownPatch(ref float ___currentDayTime, ref SelectableLevel ___currentLevel, ref float ___normalizedTimeOfDay, ref float ___totalTime, ref int ___numberOfHours, ref float ___globalTime, ref int ___votesForShipToLeaveEarly)
		{
			PatchGlobalTime = ___globalTime;
			PatchcurrentLevel = ___currentLevel;
			PatchTotalTime = ___totalTime;
			PlayersInMatch = StartOfRound.Instance.connectedPlayersAmount + 1;
			int num = ((PlayersInMatch <= 1) ? 1 : (StartOfRound.Instance.connectedPlayersAmount + 1 - StartOfRound.Instance.livingPlayers));
			LeavingEarly = ___votesForShipToLeaveEarly >= num;
			if (!LeavingEarly)
			{
				ShipLeavePerdiction = (___normalizedTimeOfDay + 0.1f) * ___totalTime;
				ShipLeavePerdictionNormalized = ___normalizedTimeOfDay + 0.1f;
			}
			if (LCcountdownMod2.DebugMode)
			{
				if (DebugTimer > 0f)
				{
					DebugTimer -= Time.deltaTime;
				}
				else
				{
					Debug.Log("-GAMETIME-");
					Debug.Log("ClockTime " + GetClock(___normalizedTimeOfDay, ___numberOfHours, createNewLine: false));
					Debug.Log("ShipLeaveClockTimePerdiction " + GetClock(ShipLeavePerdictionNormalized, ___numberOfHours, createNewLine: false));
					Debug.Log("NormalizedDayTime: " + ___normalizedTimeOfDay);
					Debug.Log("ShipLeavePerditionNormal: " + ShipLeavePerdictionNormalized);
					Debug.Log("CurrentDayTime: " + ___currentDayTime);
					Debug.Log("ShipLeavePerdition: " + ShipLeavePerdiction);
					Debug.Log("NumberOfHours: " + ___numberOfHours);
					Debug.Log("TotalTime " + ___totalTime);
					Debug.Log("connectedPlayers: " + PlayersInMatch);
					Debug.Log("-COUNTDOWN-");
					Debug.Log("EarlyCountDownTime: " + EarlyCountDownTime);
					Debug.Log("LeavingEarly: " + LeavingEarly);
					Debug.Log("CountingDown: " + CountingDown);
					Debug.Log("DownTimer: " + DownTimer);
					Debug.Log("Number: " + Number);
					DebugTimer = 0.1f;
				}
				keyAction.Enable();
				keyAction.performed += delegate
				{
					DisableDisabler = true;
					DownTimer = 3f;
					Number = 0;
					CountingDown = true;
					//Countdown
				};
			}
			if (___currentDayTime >= 1032f && ___currentDayTime <= 1035f && !CountingDown)
			{
				DisableDisabler = false;
				DownTimer = 3f;
				Number = 0;
				CountingDown = true;
				//Countdown
			}
			if (LeavingEarly && !CalcuatedWhenToCountdown)
			{
				CalcuatedWhenToCountdown = true;
				EarlyCountDownTime = ShipLeavePerdiction - EarlyCountDownDuration;
			}
			if (___currentDayTime >= EarlyCountDownTime && ___currentDayTime <= EarlyCountDownTime + 3f && LCcountdownMod2.StartCountdownWhenShipLeaveEarly && LeavingEarly && CalcuatedWhenToCountdown && ___currentDayTime >= 120f && !CountingDown)
			{
				DisableDisabler = false;
				DownTimer = 3f;
				Number = 0;
				CountingDown = true;

			}
			if (CountingDown)
			{
				if (DownTimer > 0f)
				{
					DownTimer -= Time.deltaTime * ___currentLevel.DaySpeedMultiplier;
				}
				else if (Number < LCcountdownMod2.Txts.Length)
				{
					LCcountdownMod2.CountDownInstace.Count(LCcountdownMod2.Txts[Number],LCcountdownMod2.TxtSizes[Number]);
					Number++;
					DownTimer = 3f;
				}
				else
				{
					StopCountdown();
				}
				if (___currentDayTime >= 1079f && LCcountdownMod2.StopCountdownAfterTwelve && !DisableDisabler)
				{
					StopCountdown();
				}
				if (StartOfRound.Instance.shipIsLeaving && LCcountdownMod2.StopCountdownAfterShipLeaves && !DisableDisabler)
				{
					StopCountdown();
				}
				if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && LCcountdownMod2.StopCountdownAfterDeath)
				{
					StopCountdown();
				}
			}
		}

		public static void StopCountdown()
		{
			Number = 0;
			DownTimer = 0f;
			CountingDown = false;
			DisableDisabler = false;
			CalcuatedWhenToCountdown = false;
		}

		public static string GetClock(float timeNormalized, float numberOfHours, bool createNewLine = true)
		{
			int num = (int)(timeNormalized * (60f * numberOfHours)) + 360;
			int num2 = (int)Mathf.Floor(num / 60);
			string newLine = "";
			string amPM = "";
			newLine = (createNewLine ? "\n" : " ");
			amPM = newLine + "AM";
			if (num2 >= 24)
			{
				return "12:00\nAM";
			}
			amPM = ((num2 >= 12) ? (newLine + "PM") : (newLine + "AM"));
			if (num2 > 12)
			{
				num2 %= 12;
			}
			int num3 = num % 60;
			return $"{num2:00}:{num3:00}".TrimStart(new char[1] { '0' }) + amPM;
		}

		public static float PlanetTime()
		{
			return (PatchGlobalTime + PatchcurrentLevel.OffsetFromGlobalTime) * PatchcurrentLevel.DaySpeedMultiplier % (PatchTotalTime + 1f);
		}
	}
}
