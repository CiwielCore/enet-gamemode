using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace eNetwork.World
{
    public class IPLs
    {
        public static void Initialize()
        {
            NAPI.World.RequestIpl("ch1_02_open"); // Глитч открытого интерьера на пляже
            NAPI.World.RequestIpl("sp1_10_real_interior"); // открытый интерьер стадика
            NAPI.World.RequestIpl("sp1_10_real_interior_lod"); // открытый интерьер стадика
            NAPI.World.RequestIpl("gr_case6_bunkerclosed"); // закрытый бункер merryweather
            NAPI.World.RequestIpl("Coroner_Int_On"); // части интерьера больницы
            NAPI.World.RequestIpl("ex_dt1_02_office_02c"); // аркадиус
            NAPI.World.RequestIpl("imp_dt1_02_modgarage"); // аркадиус гараж

            // CASINO ***************************************
            NAPI.World.RequestIpl("hei_dlc_windows_casino");
            NAPI.World.RequestIpl("hei_dlc_casino_door"); 
            NAPI.World.RequestIpl("vw_dlc_casino_door"); 
            NAPI.World.RequestIpl("vw_casino_garage"); 
            NAPI.World.RequestIpl("hei_dlc_casino_aircon");
            NAPI.World.RequestIpl("vw_casino_penthouse");
            // **********************************************

            NAPI.World.RequestIpl("bh1_47_joshhse_unburnt");
            NAPI.World.RequestIpl("bh1_47_joshhse_unburnt_lod");
            NAPI.World.RequestIpl("CanyonRvrShallow");
            NAPI.World.RequestIpl("Carwash_with_spinners");
            NAPI.World.RequestIpl("fiblobby");
            NAPI.World.RequestIpl("fiblobby_lod");
            NAPI.World.RequestIpl("apa_ss1_11_interior_v_rockclub_milo_");
            NAPI.World.RequestIpl("hei_sm_16_interior_v_bahama_milo_");
            NAPI.World.RequestIpl("hei_hw1_blimp_interior_v_comedy_milo_");
            NAPI.World.RequestIpl("ex_dt1_02_office_01b");

            NAPI.World.RequestIpl("gr_grdlc_int_02");
            NAPI.World.RequestIpl("gr_grdlc_int_01");
            NAPI.World.RequestIpl("grdlc_int_01_shell");
            NAPI.World.RequestIpl("gr_entrance_placement");
            NAPI.World.RequestIpl("gr_grdlc_interior_placement");
            NAPI.World.RequestIpl("gr_grdlc_interior_placement_interior_0_grdlc_int_01_milo_");
            NAPI.World.RequestIpl("gr_grdlc_interior_placement_interior_1_grdlc_int_02_milo_");


            NAPI.World.RequestIpl("gr_case0_bunkerclosed");
            NAPI.World.RequestIpl("gr_case1_bunkerclosed");
            NAPI.World.RequestIpl("gr_case2_bunkerclosed");
            NAPI.World.RequestIpl("gr_case3_bunkerclosed");
            NAPI.World.RequestIpl("gr_case4_bunkerclosed");
            NAPI.World.RequestIpl("gr_case5_bunkerclosed");
            NAPI.World.RequestIpl("gr_case6_bunkerclosed");
            NAPI.World.RequestIpl("gr_case7_bunkerclosed");
            NAPI.World.RequestIpl("gr_case8_bunkerclosed");
            NAPI.World.RequestIpl("gr_case9_bunkerclosed");
            NAPI.World.RequestIpl("gr_case10_bunkerclosed");
            NAPI.World.RequestIpl("gr_case11_bunkerclosed");

            NAPI.World.RequestIpl("k4mb1_ornate_bank_milo_");


            // REMOVED IPLS *********************************
            NAPI.World.RemoveIpl("sf_dlc_fixer_hanger_door");
            NAPI.World.RemoveIpl("sf_dlc_fixer_hanger_door_lod");
            NAPI.World.RemoveIpl("sf_musicrooftop");
            NAPI.World.RemoveIpl("sf_phones");
            NAPI.World.RemoveIpl("sf_franklin");
            NAPI.World.RemoveIpl("sf_mansionroof");
            NAPI.World.RemoveIpl("sf_plaque_hw1_08");
            NAPI.World.RemoveIpl("sf_plaque_bh1_05");
            NAPI.World.RemoveIpl("sf_plaque_kt1_08");
            NAPI.World.RemoveIpl("sf_plaque_kt1_05");
            NAPI.World.RemoveIpl("sf_fixeroffice_bh1_05");
            NAPI.World.RemoveIpl("sf_fixeroffice_kt1_05");
            NAPI.World.RemoveIpl("sf_fixeroffice_hw1_08");
            NAPI.World.RemoveIpl("hei_bi_hw1_13_door");
            // **********************************************

            // PILLBOX HOSPITAL *****************************
            NAPI.World.RemoveIpl("rc12b_fixed");
            NAPI.World.RemoveIpl("rc12b_destroyed");
            NAPI.World.RemoveIpl("rc12b_default");
            NAPI.World.RemoveIpl("rc12b_hospitalinterior_lod");
            NAPI.World.RemoveIpl("rc12b_hospitalinterior");
            // **********************************************

            // OBJECTS **************************************
            NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("prop_billb_frame04b"), new Vector3(-184.6184, -1163.129, 33.08752), 10);
            NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("sc1_props_combo0915_04_lod"), new Vector3(-180, -1166, 33.08752), 10);
            // **********************************************
        }
    }
}
