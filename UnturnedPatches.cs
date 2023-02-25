using HarmonyLib;
using Rocket.Core.Logging;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace MFMChecker
{
    class UnturnedPatches
    {
        private static Harmony harmony;
        private static string harmonyId = "Jdance.MFMChecker";

        public static void Init()
        {
            try
            {
                harmony = new Harmony(harmonyId);
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }
        public static void Cleanup()
        {
            try
            {
                harmony.UnpatchAll(harmonyId);

            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }


    }

    [HarmonyPatch(typeof(BarricadeManager))]
    [HarmonyPatch("SendRegion")]
    class ServerSetBedInteralPatch
    {
        [HarmonyPrefix]
        internal static bool SendRegionPrefix(SteamPlayer client, BarricadeRegion region, byte x, byte y, NetId parentNetId, float sortOrder)
        {

            var bindingFlags1 = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var serverData = typeof(BarricadeDrop).GetField("serversideData", bindingFlags1);

            var bindingFlags2 = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var sendField = typeof(BarricadeDrop).GetField("SendMultipleBarricades", bindingFlags2);
            ClientStaticMethod SendMultipleBarricades = (ClientStaticMethod)sendField.GetValue(null);


            if (region.drops.Count > 0)
            {
                byte packet = 0;
                int index = 0;
                int count = 0;
                while (index < region.drops.Count)
                {
                    int num = 0;
                    while (count < region.drops.Count)
                    {
                        BarricadeData data = (BarricadeData)serverData.GetValue(region.drops[count]);
                        num += 44 + data.barricade.state.Length;
                        count++;
                        if (num > Block.BUFFER_SIZE / 2)
                        {
                            break;
                        }
                    }
                    try
                    {
                        SendMultipleBarricades.Invoke(ENetReliability.Reliable, client.transportConnection, delegate (NetPakWriter writer)
                        {
                            writer.WriteUInt8(x);
                            writer.WriteUInt8(y);
                            writer.WriteNetId(parentNetId);
                            writer.WriteUInt8(packet);
                            writer.WriteUInt16((ushort)(count - index));
                            writer.WriteFloat(sortOrder);
                            for (; index < count; index++)
                            {
                                BarricadeDrop barricadeDrop = region.drops[index];
                                BarricadeData serversideData = (BarricadeData)serverData.GetValue(barricadeDrop);
                                InteractableStorage interactableStorage = barricadeDrop.interactable as InteractableStorage;
                                writer.WriteGuid(barricadeDrop.asset.GUID);
                                if (interactableStorage != null)
                                {
                                    byte[] array;
                                    if (interactableStorage.isDisplay)
                                    {
                                        byte[] bytes = Encoding.UTF8.GetBytes(interactableStorage.displayTags);
                                        byte[] bytes2 = Encoding.UTF8.GetBytes(interactableStorage.displayDynamicProps);
                                        array = new byte[20 + ((interactableStorage.displayItem != null) ? interactableStorage.displayItem.state.Length : 0) + 4 + 1 + bytes.Length + 1 + bytes2.Length + 1];
                                        if (interactableStorage.displayItem != null)
                                        {
                                            Array.Copy(BitConverter.GetBytes(interactableStorage.displayItem.id), 0, array, 16, 2);
                                            array[18] = interactableStorage.displayItem.quality;
                                            array[19] = (byte)interactableStorage.displayItem.state.Length;
                                            Array.Copy(interactableStorage.displayItem.state, 0, array, 20, interactableStorage.displayItem.state.Length);
                                            Array.Copy(BitConverter.GetBytes(interactableStorage.displaySkin), 0, array, 20 + interactableStorage.displayItem.state.Length, 2);
                                            Array.Copy(BitConverter.GetBytes(interactableStorage.displayMythic), 0, array, 20 + interactableStorage.displayItem.state.Length + 2, 2);
                                            array[20 + interactableStorage.displayItem.state.Length + 4] = (byte)bytes.Length;
                                            Array.Copy(bytes, 0, array, 20 + interactableStorage.displayItem.state.Length + 5, bytes.Length);
                                            array[20 + interactableStorage.displayItem.state.Length + 5 + bytes.Length] = (byte)bytes2.Length;
                                            Array.Copy(bytes2, 0, array, 20 + interactableStorage.displayItem.state.Length + 5 + bytes.Length + 1, bytes2.Length);
                                            array[20 + interactableStorage.displayItem.state.Length + 5 + bytes.Length + 1 + bytes2.Length] = interactableStorage.rot_comp;
                                        }
                                    }
                                    else
                                    {
                                        array = new byte[16];
                                    }
                                    Array.Copy(serversideData.barricade.state, 0, array, 0, 16);
                                    writer.WriteUInt8((byte)array.Length);
                                    writer.WriteBytes(array);
                                }
                                else
                                {
                                    writer.WriteUInt8((byte)serversideData.barricade.state.Length);
                                    writer.WriteBytes(serversideData.barricade.state);
                                }
                                writer.WriteClampedVector3(serversideData.point, 13, 11);
                                writer.WriteUInt8(serversideData.angle_x);
                                writer.WriteUInt8(serversideData.angle_y);
                                writer.WriteUInt8(serversideData.angle_z);
                                writer.WriteUInt8((byte)Mathf.RoundToInt((float)(int)serversideData.barricade.health / (float)(int)serversideData.barricade.asset.health * 100f));
                                writer.WriteUInt64(serversideData.owner);
                                writer.WriteUInt64(serversideData.group);
                                writer.WriteNetId(barricadeDrop.GetNetId());
                            }
                        });
                        packet++;
                    }
                    catch
                    {
                        Logger.Log("CAUGHT AN ISSUE IN BARRICADEMANAGER.SENDREGION DUE TO STATE ARRAYS.");
                    }
                }
            }
            else
            {
                SendMultipleBarricades.Invoke(ENetReliability.Reliable, client.transportConnection, delegate (NetPakWriter writer)
                {
                    writer.WriteUInt8(x);
                    writer.WriteUInt8(y);
                    writer.WriteNetId(NetId.INVALID);
                    writer.WriteUInt8(0);
                    writer.WriteUInt16(0);
                });
            }

            return false;
        }
    }
}