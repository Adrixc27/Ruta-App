namespace RutaApp.Blazor.Data;

// Archivo generado a partir de los planos oficiales en PDF (Rutas.pdf y BOWI.pdf).
// Los PDF son GeoPDF (UTM 13N): el recorrido se tomó de la capa de la línea y se pasó a [lat, lng]
// con la georreferencia del propio plano (error de ajuste menor a 1 m), simplificado a 2.5 m.
// Los recorridos van codificados como "polyline" (precisión 6) para no ocupar miles de líneas; se leen con Decodificar().
public static partial class TrazosOficiales
{
    /// <summary>Campesina - Santa Rosa (línea diametral, cuenca Poniente): 36.421 km, 9 unidades, cada 14 min, ciclo de 126 min.</summary>
    public static readonly RutaOficial CampesinaSantaRosa = new RutaOficial
    {
        Nombre = "Campesina - Santa Rosa",
        Variante = "Santa Rosa",
        Frecuencia = 14,
        LongitudKm = 36.421,
        Flota = 9,
        CicloMin = 126,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Perif. de la Juventud y C. Bugambilias", 28.605708, -106.102096),
            new PuntoControl("B", "Perif. de la Juventud y Av. Hacienda del Valle", 28.633661, -106.121123),
            new PuntoControl("C", "Perif. de la Juventud y Av. Politécnico Nacional", 28.621146, -106.110656),
            new PuntoControl("D", "Priv. Int. Politécnico Nal. y C. 14 de Julio", 28.621646, -106.093479),
            new PuntoControl("E", "Av. Ocampo y Av. Niños Héroes", 28.636022, -106.080873),
            new PuntoControl("F", "Av. Venustiano Carranza y C. Escorza", 28.638512, -106.072267),
            new PuntoControl("G", "C. Mina Princesa y C. Mina Prieta", 28.627569, -106.038646),
            new PuntoControl("H", "C. 35 y C. Tamborel", 28.635512, -106.050013),
            new PuntoControl("I", "C. Nardos y C. Violetas", 28.617766, -106.095230)
        },
        Trazo = Decodificar(
            "yq}pu@br}jiE_jEzjAw^vHse@xScYjXkOl[oWnq@kcA~kBah@jv@cYxVyLlEyLfBgNDgN_Bwh@}Ro]qFseAVuO_BgNoCwaAc\\kYsFwe@Lk]hJc" +
            "YtScNjPuf@`{@mO`R{kC`yEiV|Yi@zFqD@gRlPebBz~@mVnPcd@j`@uiBt~CmObU_`@v^ih@t[_r@bWag@nM{cGplBut@fe@{WfJmVnEd@qV~J" +
            "sw@`Vst@tLuVz_@wi@hjB}tAtSeJDtYhCxQvKb[~kArbBnYlYbbCwu@~|@aj@le@uf@`|AqhCjZkc@vLgMrHqH`jCs~Aja@{a@xPeUt{DahHtL" +
            "mP|EwChRsHvSkB`Jl@b\\~DthAd_@xe@zDj}@\\|W~Dxa@zOxS`E|WGzWqE|[_O`Ye`@d{@_vArhAqnClOqSlVcRbnEolA`aB}`@fYmBz^xAf`Bb" +
            "b@fC|I~EvFbJpCjRw@bCeBlD_JA_JiCcMiN}LgN_BaJfBiyAq`@wl@yDgk@hGuOcErTq_A?uCyAcBolKciDacDshAq]uIys@qWod@wLwzBky@a" +
            "nGauBbvBmvIm@yFkdEwqAmdAmWu~AuMi`@qFmKv@am@go@coDqdEeXga@wO_McbAoqAiGr@eR|Y_Nj[em@|nBgG|FuHxCid@tEkfBqpBsgAkcA" +
            "goFwbGv}AiaBrcAiqAbXlYdC`BpDAzu@{_ArSsSzEu@~b@Ip]|DjrBjv@zShKhg@zOpo@fNdCs@`]{l@b~BozE`mJifSyAgEcQ}LemFiiDk{M}" +
            "yI`]cs@tkCxfBdCArf@{bAzr@i_Bhe@q|@bCeBrLk[jn@|]f\\jYzdEvsCzEs@jOq^xP{YdG}QtIwo@xIa`@rEif@a{DigChZas@dU|Ate@kJhU" +
            "`PxLElOyYxEkPhZek@o@_UyDw\\mUqg@_Ts\\uYil@gxAiqAqD@gGjEcCzFDbX`FnKlQzx@uEh[qHtVcGhP{|@~_AyFl_Aki@rgAuAlHBfPw_@p|" +
            "@yPl[i@zFsLfXcCdBaw@d|A}aAbzBukCyfB}Er@ge@zbA@zF`oS~{Mki@`fAydC~nFedBpiDodFnnKsLj[_iAxeBmoBhjCe~@tdAoDxCsmAle@" +
            "{uCibDnjAgnAjUhVsHrK_Gjf@k_AbyA|K~x@nGvQbt@j`AptC`gDnDAzj@mv@nKCjmBfuB|b@cDlKyCpOqHbNwKtg@shBhKoS|l@dY`XvT~`Cb" +
            "nCpfC~xCrRr\\`l@v|AnUzb@vHvFxOnYb[pr@jiAfzBvHbEpv@bNjYhK_VdaA@tCvAbBv~Gv|BufA`rExAxFf`Crt@jvHngC|Eu@|UgdAfnB`p@" +
            "bCAvAeBpk@_hCd`Fp_BaZvmA")
    };

    /// <summary>Panamericana - San Felipe (línea radial, cuenca Norte): 22.115 km, 5 unidades, cada 15 min, ciclo de 75 min.</summary>
    public static readonly RutaOficial PanamericanaSanFelipe = new RutaOficial
    {
        Nombre = "Panamericana - San Felipe",
        Variante = "San Felipe",
        Frecuencia = 15,
        LongitudKm = 22.115,
        Flota = 5,
        CicloMin = 75,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "C. Domingo Sarmiento y Av. Universidad", 28.654552, -106.090087),
            new PuntoControl("B", "Av. Washington y C. Ing. Enrique Rubio", 28.647965, -106.117485),
            new PuntoControl("C", "C. Montes Americanos y UTCH", 28.643697, -106.145996),
            new PuntoControl("D", "Av. Washington y C. Ing. Enrique Rubio", 28.647667, -106.117065),
            new PuntoControl("E", "Av. Niños Héroes y Av. Ocampo", 28.636518, -106.080778),
            new PuntoControl("F", "Av. Niños Héroes y Av. Cristóbal Colón", 28.647002, -106.069020),
            new PuntoControl("G", "Av. Cristóbal Colón y Av. Pascual Orozco", 28.661530, -106.085762)
        },
        Trazo = Decodificar(
            "{l}su@x|ejiE~\\dc@in@ru@@hFzBzA||@dDrtCA|f@yCf_@pA`wFjx@nv@dGfl@vHbuDlj@uYno@wLfu@ie@tl@}Ul_@{GpS_@r`@lPpsA}Z~p" +
            "@i\\ti@ks@jaAgN~WyInbBp@b_@gNl\\gTbnAeOxfAqKni@aDjFuX~J}Tg@yOcFyFgF_PcYcD@qA|AayApoN{c@~~ErAzDdDAlcD{iAbi@fiBlDd" +
            "\\ym@pqBwVprAuBlVuJvrC|MnyAhRhuCpLxmAbBv`AiGfxAeEf_@qKbe@k]neAw`@`}BuB~TLbr@ta@tcChQl`@lVl]vJpJpq@``@~XbVzOxQhg" +
            "BhdCbLvWfHp]rWjMbD_BmD}d@eL__@oImPa|Bo{C{i@cc@_UmJ_LaI_LaL{SoZoMmc@gPyw@iL}t@q@u]bE_h@`b@gwBda@kkAhKkbAtC_{@}@" +
            "oiAcNmoBb@_ReDuGwTgeCgDsJsHu`AAeL|C{WzEihBnUcqAfIyJ|i@ggBtB{Wk@sMgm@c_CeDyD}_GtvB_\\elAcCq`@xlC}uWznAhsChD~N|Ot" +
            "WvS|KjRrDvXeBzT}JrFyGjEyJtj@wqDnOk_@lAeLo@e\\pHu|AjS{]xg@sr@la@aq@f\\mr@f@kCoPqvA`@iYrFaRrT}]rf@wl@~Msy@n]ku@i@i" +
            "F{B{A}|Dyk@iM_LcH_OCeI`DwJdoFgwG|GyJtK}WvLex@tKaUpgEklFfw@ybAd@{Dw`@i]_s@qt@mcEiuEggE{sEud@ua@ccBcjBo}@_fA{Bm@" +
            "}qCrhDk|@jaAsjTfqW@xGfwClhDh@zDv~@|eAzB?f^va@v~@lgApRbYlEAbDoCnoAowA")
    };

    /// <summary>Panamericana - Mirador (línea radial, cuenca Norte): 20.971 km, 5 unidades, cada 15 min, ciclo de 75 min.</summary>
    public static readonly RutaOficial PanamericanaMirador = new RutaOficial
    {
        Nombre = "Panamericana - Mirador",
        Variante = "Mirador",
        Frecuencia = 15,
        LongitudKm = 20.971,
        Flota = 5,
        CicloMin = 75,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Av. Niños Héroes y C. Mariano Monterde", 28.635911, -106.080892),
            new PuntoControl("B", "Av. Washington y C. Ing. Enrique Rubio", 28.648185, -106.117501),
            new PuntoControl("C", "Perif. de la Juventud y C. Sierra de la Campana", 28.626415, -106.115384),
            new PuntoControl("D", "Av. Washington y C. Ing. Enrique Rubio", 28.648185, -106.117501),
            new PuntoControl("E", "Blvd. Ortiz Mena y C. Maryland", 28.636036, -106.097937)
        },
        Trazo = Decodificar(
            "ekxru@butiiEub@we@~}@qeAf]zi@htAfmBtsBtbAekA`uBwiFvvJu~@h`B^dEnx@rk@j}Ab@oSphGuwBfAib@rGuvCfbBs]|V_QtP~Kzb@qAt" +
            "UeEnP{Oll@i]dbAoaAzmAgElM}F|]aIx`Bus@pjByG|]iCxYSz^tU~rB^hKmBtVsk@aSkm@}K}}@og@}kCoa@oCbB_f@vcFk]zbDePlsBpC~@l" +
            "|EaeBxJgBhOCvCtfCS`e@uXffA_Yjf@qBhIkAbf@gMbb@oAtW^hIjc@~wBvMpXpNdMpCdFxAdG~@pTa^trAeOvUs]`Yu@jLtBfH~H~CbQmHjVy" +
            "TzYoi@`VaaA~O{X|Z_YduGg}Cfb@uJ~Rg@`Rx@t|AxXrVxBx]kBr^uLrp@se@ha@{R~iIehCzm@k^fs@}q@tiBk{CzZe_@nLmJ~GmJv@eF}@kK" +
            "oDaE}H]gFb@_[zUs\\fa@ijA|pB}HfEk^lg@gX|Wki@f\\sq@dVgbHhwBmVpKye@f\\eYpK{\\pFgZhA}cBuXoj@NkXlEePjFipA`l@qrB`cA{jA|g" +
            "@s]d^ePhDyJ}AkEaD_JgKsOu\\u_@yqBt@oOzOig@lAae@lCkJhVea@rYiiAl@ei@yC_pCy@aAkb@J_\\rIw@`Bw]vNgfDzkA_d@|Rcd@xM}Xq`A" +
            "sFci@hvAtRna@xErBa@dQioBpBgFdy@m}Hd_Cn]~|@ng@pk@|Jdp@bVzIeAlCoOlA}^gM}jAwDcj@fAos@zG{[pt@qjB`E_b@~By}@|F{[bXgc" +
            "@tn@}r@|g@ebBdMgh@r@qQ}@oRoEkNdu@m_@xoBimAbZsLx]oE`xD}@a@qTbL{jDfG}dBnBmNn~Cw_G]aCmgCwsCvbCcoE{@aB_]}Gkb@uAq`@" +
            "h@iv@|KqoBevB")
    };

    /// <summary>Ramiro Valles Concordia - Único (línea diametral, cuenca Poniente): 52.633 km, 14 unidades, cada 11 min, ciclo de 154 min.</summary>
    public static readonly RutaOficial RamiroVallesConcordia = new RutaOficial
    {
        Nombre = "Ramiro Valles Concordia - Único",
        Variante = "Único",
        Frecuencia = 11,
        LongitudKm = 52.633,
        Flota = 14,
        CicloMin = 154,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Carretera Aldama - Chih. y C. Séptima", 28.718333, -105.998021),
            new PuntoControl("B", "C. de la Quinta y C. de la Laguna", 28.677785, -106.003948),
            new PuntoControl("C", "C. de la Nogalera y C. del Carruaje", 28.666403, -106.014912),
            new PuntoControl("D", "Av. Juárez y C. 37a.", 28.647109, -106.066981),
            new PuntoControl("E", "Av. Niños Héroes y C. Mariano Monterde", 28.635479, -106.081675),
            new PuntoControl("F", "Av. 20 de Noviembre y C. 28a.", 28.622408, -106.074055),
            new PuntoControl("G", "C. Méndez y C. 64a.", 28.606701, -106.081834),
            new PuntoControl("H", "C. 70a. y C. Urueta", 28.599926, -106.076961),
            new PuntoControl("I", "C. Méndez y C. 64a.", 28.606701, -106.081834),
            new PuntoControl("J", "Av. 20 de Noviembre y C. 28a.", 28.622408, -106.074055),
            new PuntoControl("K", "Av. Niños Héroes y C. 3a.", 28.639252, -106.077646),
            new PuntoControl("L", "Av. Juárez y C. 37a.", 28.647109, -106.066981),
            new PuntoControl("M", "Perif. Lombardo Toledano y C. 3a.", 28.647548, -106.021326),
            new PuntoControl("N", "C. de la Nogalera y C. del Carruaje", 28.666403, -106.014912)
        },
        Trazo = Decodificar(
            "akywu@|rrdiEnbDzvAvs@f_@jYtJd{Ij}D~qTxrJjfH`zCftFteCpIClIaEru@_jAfrDyiGp|A_cCjI}PjqB}fDjV_]jV}e@rN_TjYrSxcAz_A" +
            "~`A~p@jDzJhDzDtA|GrAA|FzJnQtMtA|DbTtPhq@bt@dTtP`d@je@rmDbgDek@|}@@zMrA?@~AdLxGjkBfzAhi@he@hi@jb@rAA?~AbLC?_BrA" +
            "??_BjV}_@nVcTv`@}q@dD}JfDzAjcCxcCzFzA?~AzFA?_BfDaB|K}Vbx@}uAfD_Bd^}n@drA_qBrA?fQaQzbAgu@bd@hk@wh@bc@w[bTwF~Miq" +
            "@ct@uA}DsA??}AoI@gD~DsA@qA|Daf@`o@kp@|fAkI~JsbDv}F@~AsA?@|JrAAtA|Dzs@bn@zbDlfCdTtS|f@`iApa@fw@rAAbd@je@rfI|~Fp" +
            "QrVtA|GpYlb@hD|DrA??|AnIA?_BrAAlI_KvKwn@vqEhpArhBlhA@~AvNEvF}P`D{Vv]qaBpXq~AdD}J~~@_yEA}JsA@A_BoI{DiD@eD`B?|As" +
            "A@yb@j_Co|@bjEqXn~AyKzb@uFx_@y]r{A_VroAqSxw@aL~McD|JeQ~Y?~AsA?i@vkD~i@r~BvAxPJtk@kAx\\aDzYBxP_Dxb@uCpcA_Dvb@gIz" +
            "_@@|DgtAn{DiI|SeQ|YiQbKks@jQaL`K?|AsA@cD|PDxVjDxMrA?@|A|FzGrA?jfAxy@bTtMfLvP`GxPbGrb@cApz@xAv\\`GxSrAAhLvSdLvJl" +
            "QvDtrC~gAfa@pJxxArbAsiB`fCki@gk@sA??}AqI@@~AsA?ek@~z@m}@bjAsbA~uAgD`B@|GsA??~ArAA@~DrAAd\\n_@lfAvbAdvBzzBv^l\\hs" +
            "Cj{CrAA@~AzFCfy@~v@b~BvcCrAAx{@~s@ji@fk@rn@dk@r~@xbAgD~D@~AsA?qFxe@_f@|w@kV`]?|AsA@rLj}@`GvSrA?pYlb@nnAnwAtA|D" +
            "zkAptAfLvPfDA@~AzFCA}ArAAfk@_u@fDAfLvMfD|AfTpYfLvJ`l@dq@pIxG?~A~h@mBnIaEfDAdD_B?_BnIaB~KaKrA??_EvF}Ml`@uxA|X}e" +
            "@lIaHrA?xVpStA|DdTrShYrJvNCnIcB?}AbYcQdQaTpVcNA}AxSaZxe@yiAjI_Nhu@ujBlIaHvNEtVvDrVgBvNcEA}AzFaB~KaKzFaB?}ArAAb" +
            "D}J|X}h@vF}S`^{t@~K_Q`\\nVbTtJvcAzs@zpAvy@jfAxv@rA?fLtPty@poBtf@h_@j~@`k@fDA?_BfD_Bl[{t@xFaHv_AwpBfLvJ|rCthB~c@" +
            "j\\veCz|A?~A`\\pP~jCveBdLCA_BrA?xr@y~AvS}_@t_AwsBleDjqB?~AfDA@~AlIcBpA}GdQ_ZvF}Pp[}k@vr@w{Ap[}k@lA{SgD}DqIBqgAxv" +
            "Bc}@vpBsA?}k@gb@cy@ah@k~@ah@A}AoI@@~AsA??~Asz@xaBoN|_@ic@zz@m[zt@}k@gb@a\\qPcy@ah@g_G}wDspBetAgD@A_BgD@?~AsA?eD" +
            "~Ds`@z}@qN~VwF|PwNbBipCukBeTsMcL{D_GyGcvAs|@o~@_q@s^oSkkBgqAwVuMA}AgD@eD~A?~AsA?qA|GyF`H}K|V_L~Pqm@vxAgD`B{X|k" +
            "@cLBaTwD_TdE@|AsA@?|AeQ~Vye@xlAku@xaB}X~e@cY`TkI`KoI`EyF~GcLB{F{AeTuPuA}DoQsP{F}AiD{DyNyDsVdE@~AsA?mI~M@~AsN~S" +
            "wF|M_Qxn@sA@eD|J@~DgIzYkI|SeD~DoI`EmIbBkQDgD~A}NuSysAmzAii@ge@{cAwhAiD}AofAsnAsI{Gmi@eq@gD{AqfAukAsA}D}NuPgD}A" +
            "cfCqoC}s@aq@avCgdDfD_Bzm@cu@GwYpA_HsA@?}DsA?A_BgD{AgTsVcaA{_A{cAwhAwkAueAsIwM_yAmwAa{DuaEuf@ib@aqAqtAiLwPA{J~K" +
            "_QrAAte@u{AdD_HrA?rF{Y|o@qbC||@qhChV}k@dbAsnC`Qyk@hgAq}CtF{Y`KchChKigBGwb@sLm}@e_@_uAl@{|CvF}MjV_`@|K}YtaAavEr" +
            "Fy_@dD_H`m@g}ChNwz@vo@g}ClSufAA}JsA@A_BoI{DiD@eD`B?|AsA@__C|xLcy@an@}c@oP?_BytDugAgD@qf@oP{FB@|AgD`BwKvq@qYob@" +
            "c\\o_@kwFe{DmcBkwAgt@ywA}Vmk@ugFidExm@}cAtm@}lAhp@}iAlI_Kv`@}q@r[_c@?_Bx`@_i@jq@`z@?~AbLC?_BrA?dD_EjI_TprAgdArA" +
            "AA}AtAAC{JgD}AuA}Dkq@a}@sA??}AqI@kx@fo@w[`TmI`H?~AaL~JsA@@|A{m@`x@}e@|z@sN~P?~AijAzpBefBgcBad@m_@?}AcLBeQ|_@c^" +
            "|k@}S`Q@~AsA?sN~S?~AmcBmtA}k@ib@ii@ie@rh@}z@A}DrAAA}GiD}AiD{GmdIszH}FyG}NuYsIyJeq@ge@sA@ocBmzAvS}_@rA?A}JsA??}" +
            "AuA?}F{GcLB@~AsA?qN~YqbAzdBwhCzjEq|A~_CaL~MoiB|`D}K|VmV`WsA??~A_Y~_@{S`Q}SbK_TdBgYFqcAc_@ea@qGoQyG_\\qP}zBk_Aa\\" +
            "qPyaUq{Jc{Ik}D}c@qP{bCghA_y@g\\iwEcwBmQwGcLBwF|P@|Jt{@d_@")
    };

    /// <summary>Aeropuerto - Jardines de Oriente (línea radial, cuenca Sur): 34.651 km, 7 unidades, cada 14 min, ciclo de 98 min.</summary>
    public static readonly RutaOficial JardinesDeOriente = new RutaOficial
    {
        Nombre = "Aeropuerto - Jardines de Oriente",
        Variante = "Jardines de Oriente",
        Frecuencia = 14,
        LongitudKm = 34.651,
        Flota = 7,
        CicloMin = 98,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Blvd. Juan Pablo II y Perif. Lombardo Toledano", 28.644366, -106.014467),
            new PuntoControl("B", "Av. Equus y Blvd. Juan Pablo II", 28.671475, -105.998795),
            new PuntoControl("C", "Av. Equus y Av. Quinta Real", 28.670296, -105.979506),
            new PuntoControl("D", "Av. Equus y Blvd. Juan Pablo II", 28.672089, -105.997228),
            new PuntoControl("E", "C. del Carruaje y C. Los Ángeles", 28.664161, -106.011748),
            new PuntoControl("F", "Blvd. Juan Pablo II y Perif. Lombardo Toledano", 28.644366, -106.014467),
            new PuntoControl("G", "Terminal Sur", 28.624432, -106.028118),
            new PuntoControl("H", "Av. Niños Héroes y Av. Independencia", 28.638218, -106.078537),
            new PuntoControl("I", "Av. Juárez y C. 37a.", 28.647222, -106.066678),
            new PuntoControl("J", "Av. Carlos Pacheco y C. J.J. Calvo", 28.639363, -106.051887),
            new PuntoControl("K", "Blvd. Juan Pablo II y Av. Carlos Pacheco", 28.623349, -106.028055)
        },
        Trazo = Decodificar(
            "irhsu@`jreiEa__@wiQk`Ts|JgWeTgDsLAuJbaAoxBzqE{xH|EAA}@dP_EnYbdBnBxBbD?nbE_gAjeDew@lB}BeF}c@yGeZwWebBmIsJiI@?~@" +
            "oxEdnBmg@quBnGwJhBuLu@?cFc^eb@ymBv@?r@yFAwHlB_AdIuNzJoTdIoVlLah@hGah@f@oy@u@@oDwm@aFgZyIe_AcMou@q[amC{EyB?|@{u" +
            "@|V}VvPmN~DkIqLyLwDiIB?|@_K|DcDzFgBnR_WxNys@xaAa\\tT@zDncCdcNvn@xuDDhZiBtLoyBjwDyzAldCcf@v}@_|@lxB@zDfDrJhPjPlB" +
            "?zoVplLuElXrDf_AcDzF_t@lh@sjBtyA_`BnbC@xFbdCd_BzEA~iAqqAdyAjgBpaBfdApGAhNqVzn@od@`h@wVpG{Hhn@qnBp}@_hCpgPjyH|E" +
            "z@ty@nc@xaKpwEdzXjxM`RhRtGpL@vHnBvF|EvFrGxBrNhRpBtJwJ|i@aUhb@mB~@cD}@w@}@t@{BdZgf@w@wHjB{D|Qx@v@zBpb@nzCo@f^iG" +
            "d`@_f@jkAs}@dbCyJh\\eGvq@F`b@~Gro@vP|fA|@b`@oExo@Fd\\rBhXv@?hMxlAxPtnA`[zdAbs@rfAd^|Y~Gnu@yAxrApBrL~@|e@m@d^`Azi" +
            "@zG|a@nWd{@fDvFqGxHiBvJgu@|bB_oB~`E{`@r_Ayl@nkA}dC`nFyiIlyPoLj\\}l@v_AwrBjrCyVl\\ecAxgA}rAvh@_y@dv@@xFb`BbkB`s@z" +
            "|@dDA?|@?}@bDA|x@ugAA{D}_@{[ym@go@}cFevFqU_Zex@}x@amA}sAut@cs@ctCg_DoByD`DyFvn@ct@Gab@_aCgbCtlDeuE|mCm_EbD}Bd}" +
            "EccKteFsxKdNgb@~CqP`Bwo@kFeaAEkVpE_h@zOch@frAkbCvdAacBnkE{jFnfEgyErXuThLo{@ts@kqAts@okAbKnLpBpNwJ`f@kSj`@cD|B{" +
            "E{Bz[cj@w@wHlGuL?}BiPkNhNqVxE_AbD{DjBwJt@?AwHoBuJ}EuF}k@w]kuAyr@{zJuqEoB{BcWmJwSmL?_A}bFiaCozAyp@k~Jm{E")
    };

    /// <summary>Komatsu - Inverso (línea radial, cuenca Sur): 30.94 km, 6 unidades, cada 18 min, ciclo de 108 min.</summary>
    public static readonly RutaOficial KomatsuInverso = new RutaOficial
    {
        Nombre = "Komatsu - Inverso",
        Variante = "Inverso",
        Frecuencia = 18,
        LongitudKm = 30.94,
        Flota = 6,
        CicloMin = 108,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Av. Nueva España y Vialidad CH-P", 28.622869, -106.030646),
            new PuntoControl("B", "C. Elisa García y C. Margarita Flores", 28.608975, -106.039152),
            new PuntoControl("C", "Av. Nueva España y C. 76 1/2a.", 28.577814, -106.034356),
            new PuntoControl("D", "Av. Nueva España y Perif. Fco. R. Almada", 28.599073, -106.031471),
            new PuntoControl("E", "Av. Nueva España y Blvd. Fuentes Mares", 28.619790, -106.031044),
            new PuntoControl("F", "Av. Independencia y Av. Paseo Bolívar", 28.633548, -106.072414),
            new PuntoControl("G", "C. Aldama y C. 31a.", 28.644492, -106.067850),
            new PuntoControl("H", "C. Julián Carrillo y C. 4a.", 28.638311, -106.080648),
            new PuntoControl("I", "Av. 20 de Noviembre y C. 16a.", 28.625453, -106.071712)
        },
        Trazo = Decodificar(
            "y`_ru@ldrfiEfgBfUvYn@r}Ezn@fk@`K|d@zLpgAlEjaAv\\@x@va@jI`|AxH{Ahf@fj@zwAdb@v`AuiBzcAcBrB@jDpPrYbkBdyBbBA`XcQbhB" +
            "o~@vg@_\\hEA|YxNvCAtCkDdn@}KplCw^|QjBxCnBjXpYty@lp@zjB~xAhEAjHyIxb@uv@vTaj@jK{b@fNs}@rwAcnW|yFbb@aC`zAjEtItKxGz" +
            "{Ed[tCkDzWy|FrD}{Ahy@xCr^xEv|CbQwGhaB{`@ryHxCzGz`Ihj@hEAbBsBjCuk@|xArJjy@dIhXzE|Qr@{JtfBDrRwVzaE_iE__@u_AoE?y@" +
            "s_A}Aoy@gPcnD{Y{Y{Emf@_DatArAwtGqc@tJ}}Bp@?s@kDqcCuVrJoaCs@qBek@}CopByFiErB_[l`Gq^yE_h@eBarBb@sVxBi[lF_k@fQqmA" +
            "pZ{dCrN_UpDif@lOqa@`JeqDeFe{CkO{YsGm[yNwi@mPin@cBc`@aDiXmIek@aK}cFsp@qK|@eeCiZcBrB_BxYsCjM}fBjvEyIt[qClToC~`@z" +
            "@vb@dGvi@~Kbo@|C|Wv@~WmF~y@x@r[ja@bjDvPxp@xKn[|Vnb@r[lb@d]nYzHfv@oA~rAfBfMz@nd@a@baAvHvi@b_@nkAnMnk@`_@pbArzAn" +
            "dFyvGlrNq}CtsGoeBndEeo@nsAo[gKm{Cqv@a{@gPch@mPyNqIefAab@ap@}S}QcDas@g@_JrBkHfFsN`Qe]u`@iE@_jAhwAg~A}zA{y@{eAkX" +
            "yNq|AwcAeUkRun@qp@{Fw@}p@z_Ar@zG`pArnAb`A~cAn~@|s@j{@v~@p[nY~a@|e@hEnBlXpYiHpKsFrd@}e@px@_Xr]bLj}@lE`Obu@hbAnt" +
            "CdgDhEAfl@au@`J{@tlBpuBdc@}BpV}I|TiOth@klBv_@ym@lH{@~Y|UxVvW`JfDpSdDtNwBtCmDpKgFrYaZ|TiOtQeXpiAiiC~]y}@pKuK~g@" +
            "vE`U_A?y@tNoDx\\wThu@keBbBy@fa@iz@AcFq|AucAlbIkqPA}Geh@_]vF}PjmBybExIu[f@ub@oE}Wuh@uyAwbCybIgJy`@iBiMw@q[Q{bArC" +
            "aO^{kAy@i]eGgf@s@qBgh@ib@s[mb@}Vgd@wXsbAa^_sCoBsk@lFgx@iGax@gTg_BEeV~A{YnCgVhHcXleB_qEfKeh@")
    };

    /// <summary>Komatsu - Directo (línea radial, cuenca Sur): 28.45 km, 6 unidades, cada 20 min, ciclo de 108 min.</summary>
    public static readonly RutaOficial KomatsuDirecto = new RutaOficial
    {
        Nombre = "Komatsu - Directo",
        Variante = "Directo",
        Frecuencia = 20,
        LongitudKm = 28.45,
        Flota = 6,
        CicloMin = 108,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Av. Nueva España y Vialidad CH-P", 28.622829, -106.030682),
            new PuntoControl("B", "Av. Nueva España y Perif. Fco. R. Almada", 28.598655, -106.031692),
            new PuntoControl("C", "Av. Nueva España y C. 76 1/2a.", 28.577544, -106.035097),
            new PuntoControl("D", "C. Elisa García y C. Margarita Flores", 28.608937, -106.038971),
            new PuntoControl("E", "Av. Nueva España y Blvd. Fuentes Mares", 28.619775, -106.031024),
            new PuntoControl("F", "Av. Independencia y Av. Paseo Bolívar", 28.633550, -106.072468),
            new PuntoControl("G", "C. Aldama y C. 31a.", 28.644727, -106.067622),
            new PuntoControl("H", "C. Julián Carrillo y C. 4a.", 28.638269, -106.080672),
            new PuntoControl("I", "Av. 20 de Noviembre y C. 16a.", 28.625417, -106.071760)
        },
        Trazo = Decodificar(
            "y|~qu@ztqfiEyNmB{Fz@oCr[cEtRmeB~pEo@|GwFdOcEr[}AfVFvb@xZzbCDfV}Dfo@DxYhB~Wja@l_DhZpbAps@vgAb]nY|Hfv@qA~rAzClKN" +
            "h_Aq@bF|@`h@bGz`@p@?|[bfAyF`HacClqF{kBr{D_nCb~Fu~G|nNs{@plBqy@q^soA{a@c`@gBk[H_JrB_JhFaMdOe]y^{F@khAfwAivAarAm" +
            "cAunA{uBqsAofAweAyFz@ko@~}@r@|G~oApnAfx@jy@|FrInv@|l@d]f[hfB|jBv^`]kHxIcHjf@}e@px@_Xl_@bLxy@~FpRbu@p`A|rCdgDzF" +
            "Atj@yv@~IChnBpuBhf@yDzQcHnVaQbg@sjBt_@wm@hE{@zFt@rn@~l@dUtGzFt@vNuBvQoM~m@qh@tQeXfmBykElH_HvNExNfDdc@}BlHuBh^q" +
            "Vtp@k|Alg@icAAcFg`@mYg{@gi@thBaxDjnA_dCbo@yxAvzAi{C?kDeBoBsf@_]buB}rEvIo]r@?j@uRqEef@uh@uyAmgBk}Fj^sM?y@reAee@" +
            "cD}p@tAk_Ay@wYvQeXfnA{lCs@}Gw~DmhCj_B{gDfTuoBAkDeBqBgXcD~Gq}@u@cFi}B}]h~@izJlzEbm@zuAtZfaAvCpdAl^n[lIxiCvMbfD`" +
            "Hni@eA`lAi\\n_AqH|pA}FpyB{l@vYkF~}@eCngAWta@fBzd@vEtKfDnrExg@ldA`IfE{@n@oKttGje@jc@nPl[`DrrAtJryC`Ztt@rE`xC`Zpu" +
            "Ex^tCkDvJesBlOqxBu@}GxM}}BeBoB{Fu@aUs@aoBiSciAeGcBpBoYlwFsCjMkFzbAyw@iGo|@_Bq|@gParFue@{Qs@s_AdCq|Gge@wC@sCjD@" +
            "|GgMu@in@sE_`Ds]gv@wL{FBcBjDke@ltI_VroAwb@r_AoK~P}IxI{jB_yAe]i[mPiKc]a]uVkBsmDff@sVjFiXoI?w@eM|@qjCryAmHtBsNtK" +
            "ecBkpBcJ}Nx`F{qCAkDm}@e}B}pAqCoaDgOm_B}h@qgAmE_nAgWm`Cc\\obBmSqK|@aoB}V")
    };

    /// <summary>Villa Juárez - Kennedy - Fuentes Mares (línea diametral, cuenca Sur): 40.667 km, 11 unidades, cada 11 min, ciclo de 121 min.</summary>
    public static readonly RutaOficial Kennedy = new RutaOficial
    {
        Nombre = "Villa Juárez - Kennedy - Fuentes Mares",
        Variante = "Kennedy - Fuentes Mares",
        Frecuencia = 11,
        LongitudKm = 40.667,
        Flota = 11,
        CicloMin = 121,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Blvd. Fuentes Mares y C. Kennedy", 28.617523, -106.017553),
            new PuntoControl("B", "C. 16 de Septiembre y C. Jesús Tavares", 28.592128, -106.012856),
            new PuntoControl("C", "C. Sin Nombre, Torre 1", 28.580675, -105.987353),
            new PuntoControl("D", "C. Ejido Rancho de en Medio y Av. Venceremos", 28.581130, -105.980998),
            new PuntoControl("E", "C. 29a. y Av. Venceremos", 28.594603, -105.990803),
            new PuntoControl("F", "C. Abraham González y C. Sindicato de Obreros", 28.597917, -106.003179),
            new PuntoControl("G", "Blvd. Fuentes Mares y Perif. Fco. R. Almada", 28.601886, -105.992237),
            new PuntoControl("H", "Blvd. Fuentes Mares y Fco. Portillo", 28.617238, -106.016436),
            new PuntoControl("I", "Terminal Sur", 28.623801, -106.027644),
            new PuntoControl("J", "Av. 20 de Noviembre y C. 12a.", 28.626950, -106.070473),
            new PuntoControl("K", "Av. Juárez y C. 57a.", 28.655038, -106.058821),
            new PuntoControl("L", "Av. Niños Héroes y Av. Ocampo", 28.636531, -106.080745),
            new PuntoControl("M", "C. 1o. de Mayo y C. 22a.", 28.625811, -106.075543),
            new PuntoControl("N", "Vialidad CH-P y Av. Independencia", 28.619736, -106.050725)
        },
        Trazo = Decodificar(
            "kutqu@r}xeiEjdE~Obq@l@dl@bFl[GtxDfNhtAnIpcAf@jdE~Ofv@`FxfIlThBuDtCol@d{@`DjB?hByBxQ_dD|{Erd@tE}@le@_}Eu@qFm~A{" +
            "RajCqc@dy@}mNp@kJrEoHfH{nBAuDkBwBGi]aF}s@aKcp@eUcaAuQud@pJaRdGqYv\\cnGxNovBdgEs{@`SrB@z@`jB`OtJ_AbXr@~}CbV|CsFl" +
            "K{_CjpDtYv|AjKrE}@lu@weBt@AAuDkiA_nBm]ws@idAmhBbhAkz@laAiz@hByBAkJkBwBaD?wdAd~@ydAxr@{RtYq{BblBsd@`xIeyBuRoj@k" +
            "BaS`Ae~BqTkt@xLc_Dxo@mPvcCy\\flGcLj_@y`@ws@}t@a{@__@_]eXwQgSsSiG~@oEl[v@pFhgAx|@zOzOv`@jh@v[dl@lXxs@jZzuA~Ezb@j" +
            "Dzu@Jjn@eEbeAkxFgf@xNa~B~Hx@~CwDj^{sGu@qFkB{@}aAyGvFwlA`i@x^jGAnE{Sp@iLs@?AwBig@cYm`CadBcNqDov@}k@sE|@__GzgLgQ" +
            "xW}jAr~BkiAfsBahCtcF{M`RgG~QidAfsBwbArkBaiGxzKumHbnLekAl~AuJ~@sEzB_DrFeBdNr@?lBdLlBtDvJpDdNxQ`DdLcGrY@tDmYtl@i" +
            "BzBu@}@lY_g@AoHaDoFhBwD`St@~Evf@~Jph@hPjnADzSyCpY}ChLaeBhrEaLbc@kErj@Fba@~Erh@dD`NrNrjABvUiEln@Fl[r@Az@rWt@?~_" +
            "@t_DtLvd@lGbNbD`NtVxb@pQrSbI`Nh]dYlIzu@uAftAlBfL~@zd@e@haAjInj@pXly@nSh}@h_@zbAxVrw@bm@xwBbUzq@_uHrgPuzBltEmcB" +
            "n}DgG~Qcm@|pAoaE_eAgq@sMuy@}Xan@aYooAeb@aIw@ot@k@_I|B}HrF}MfPi]y`@uE@qiA|vA__B{zAaz@odA{cB{gAue@y^k}Bu|BczEabF" +
            "kG@_j@jv@wEqDgiFgoF`j@ot@AmH{o@yo@wEsD_D@su@zaAs@nHjBvBfiHdkHnl@bj@r[|^lGrDfiDdsD`Ix@dnCrkCnl@xo@h}@ls@t{@x~@r" +
            "[dYjb@lf@vEvBfXfYgGpHwHzf@sf@dz@eVr[dKl}@xE~O`u@rbA|t@dy@d_BxmBtEAtk@mv@`IAfnBduBv^cCrOyDtOuHpOmL|g@kkBxRq]fLg" +
            "N~C}@jGv@zTvQhXfYn[`JjQ}B|CwDhLuFpYyYpTiPdQyWhc@w_AbdAgfCfLoJvOElVlDtTcAtOwF|\\cThu@{eBjB}@pa@{}@wEkHmvAq~@fbIg" +
            "oPr@wDkBsDkg@}\\|`AcuBlvAl`AjG{B~EqiDlAotB`B{d@I}d@{Gi_AiI_a@_~@uyCiI_a@oByS\\}yA{@m[_Kqh@sQc]yJcLcsCcbC}j@iy@eZ" +
            "k{@mc@ggDsBqj@fE{w@}@k[gDi]}QqjAoBySEwUbBm[nEo[hByBtRqn@jp@_bBxd@ypAt@A|CeN`G}d@EuWeD{QqBk[r@sF|HmJbhBm\\`XcArJ" +
            "yD|RgPbt@ipGfe@_pFdGo[~d@ex@zH}SpOyW")
    };

    /// <summary>Villa Juárez - Ávalos - Plan de Ayala (línea diametral, cuenca Sur): 37.48 km, 11 unidades, cada 11 min, ciclo de 121 min.</summary>
    public static readonly RutaOficial PlanDeAyala = new RutaOficial
    {
        Nombre = "Villa Juárez - Ávalos - Plan de Ayala",
        Variante = "Ávalos - Plan de Ayala",
        Frecuencia = 11,
        LongitudKm = 37.48,
        Flota = 11,
        CicloMin = 121,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Blvd. Fuentes Mares y C. Kennedy", 28.617677, -106.017855),
            new PuntoControl("B", "C. Abraham González y C. Sindicato de Obreros", 28.597908, -106.003167),
            new PuntoControl("C", "C. 78a. y C. Praderas de San Guillermo", 28.573215, -106.011413),
            new PuntoControl("D", "Perif. Fco. R. Almada y C. 6 de Septiembre", 28.595779, -106.011970),
            new PuntoControl("E", "C. Fco. Portillo y C. 15a.", 28.602586, -106.017874),
            new PuntoControl("F", "C. Fco. Portillo y C. 18a.", 28.614721, -106.017070),
            new PuntoControl("G", "Terminal Sur", 28.623755, -106.027793),
            new PuntoControl("H", "Av. 20 de Noviembre y C. 12a.", 28.626890, -106.070333),
            new PuntoControl("I", "Av. Juárez y C. 57a.", 28.654997, -106.059096),
            new PuntoControl("J", "Av. Niños Héroes y Av. Ocampo", 28.636519, -106.080723),
            new PuntoControl("K", "C. 1o. de Mayo y C. 22a.", 28.625758, -106.075505),
            new PuntoControl("L", "Blvd. Fuentes Mares y Av. Independencia", 28.619727, -106.050747)
        },
        Trazo = Decodificar(
            "urtqu@rzxeiEh_@oj@lo@wqAheBatClf@a}@xR{Wr[ur@dXgb@xYsr@fh@icA`a@yv@pKsOlzAazClo@oiAleCobF|Ykf@v}@kfBf~BexEbFuU" +
            "nTcb@v]yQ~[kC`\\|Fpc@n]N|dAmGpyADrUsGx~@gG~iBeDbf@tBbCfgCfRu@nQaHBsBbCuEbiAgDtWkB~a@BnQx@`AzvGpi@vIoO`IenBAaCsB" +
            "cCEqSxeFza@na@vBnMeA`dArH|Ead@nS}kDj|BhPbH`Cj`ApDoZ|nEcKdkAkBbh@{]viF{LluAz@fGtxBhPr~@vJlqBhLlhBnPz]|FzeApFbmA" +
            "rLh}Fl]mGjqAiw@uFoa@w@i_BoNs{AmHiiK}x@qBdCmLfwCm}AoN_lBkLmiFwa@{iDmd@_cB{Z}RhCsBdEsVrgFy@`AyQn{CstDyOorAiBa\\yB" +
            "yq@q@oySan@{]{DorAiBi|BcHce@wDkeBbvCemDjsFas@rkA{oApdBaH@?`AsB@aHfEw@dEy@?u@jKz@hKdHfKfF~@nDbE~IfMtBfIx@?kKhj@" +
            "aXhf@vIqQjMuQt@iIy@cEoDaCpBeEjOz@rB`Ctb@~zCDrUmBrUcFrQ_sAnjDaOdd@y@@uIz[sGrv@Fx[x@?@fInFx_@@fG`Lbl@tDx]Dx[aDhn" +
            "@dAjp@`_@|xCnQfr@~V~k@hg@bt@`^l[nHlt@_BhsAvBjMbA~c@i@|bAjH|e@vZp`AlSjx@bz@fhC|}@x~CosCzkGmhAd}BklCl}FwR~[ut@d`" +
            "By@@}{A|tDmv@hbB_mCas@acB{^uj@aQuu@i]ecBei@sVy@{f@nCyIhGkMrOi\\s_@aHB}fAbvAe}@az@cHaEqXoYuy@idAsuBgtA}TiSkqCuqC" +
            "sdEylEaHBcj@|v@iFaEyvAuxAoDcG{kCqkC|k@_w@AeEy@??cCqn@ym@iFaC?aAmD@ox@rgAz@bEnDbEbtFnuFdr@zo@lZp]f\\lYd_EtfEtBdE" +
            "zICnpDhtDffA`~@nzCx{C@bCeFfEwGdh@of@~x@cXb^dLr|@jFjOroA|`Bp{BpiClD?zk@_w@|ICxlBduBfc@oCfFcA?aAlDA?aA`QkG?aAfOo" +
            "I`f@ijBba@un@jDeChF`Cdg@pa@jFdIzI~B@`AtK~BpMCxIgCnVwQvRwStTuOdFiGtt@c|Aj_Aa{BvImKv_@xDx]kAhOiEjDgEnVsMlv@mfBbO" +
            "yWtRgd@x@?AcCy@?{@cEgzA}aAzsDisH~y@qjBdsAyoCAgGy@_Aeg@o]faAyrBxtAz}@?`AfFcAv@eEx@?dIabIiAw`AwD}a@cJ{a@saA}dDsD" +
            "oSyBsWb@mwAaAy[sDuYiS{e@mOkSasCeaCcSoWk\\{i@aJsWwOcn@c]yrC{BuYCoQbDgj@Ich@e[g|BEsUtGqt@zGuUrk@a~AbO}[do@khBhDuU" +
            "jB{]oHmv@v@gG~GkIdZoGtq@yKnMiE|RiCbQEfOkG?aAlMmIx@cCjs@}lGfWq{CpBgGbKimA~EyYtd@}x@|GsSvR_\\")
    };

    /// <summary>Mármol - Único (línea diametral, cuenca Sur): 31.232 km, 16 unidades, cada 7 min, ciclo de 112 min.</summary>
    public static readonly RutaOficial Marmol = new RutaOficial
    {
        Nombre = "Mármol - Único",
        Variante = "Único",
        Frecuencia = 7,
        LongitudKm = 31.232,
        Flota = 16,
        CicloMin = 112,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Av. Nueva España y C. 78a 1/2", 28.576595, -106.034847),
            new PuntoControl("B", "Perif. Fco. R. Almada y Av. Nueva España", 28.598535, -106.032478),
            new PuntoControl("C", "C. Secretaría de Hacienda y C. 20a.", 28.607737, -106.046079),
            new PuntoControl("D", "C. 16a. y C. Melchor Guaspe", 28.617996, -106.058133),
            new PuntoControl("E", "C. 12a. y Av. Paseo Bolívar", 28.628817, -106.074210),
            new PuntoControl("F", "Av. Tecnológico (Cd. Deportiva)", 28.659812, -106.083651),
            new PuntoControl("G", "C. Julián Carrillo y C. 4a.", 28.638434, -106.080533),
            new PuntoControl("H", "C. 16a. y C. Melchor Guaspe", 28.617996, -106.058133),
            new PuntoControl("I", "C. Secretaría de Hacienda y C. 20a.", 28.607737, -106.046079),
            new PuntoControl("J", "Perif. Fco. R. Almada y Av. Misión de Santa Bárbara", 28.600123, -106.040268)
        },
        Trazo = Decodificar(
            "yodou@r{yfiEAiC{BiC{l@aEkc@gGmnCmUoJBu[gE_`A_Ggz@q@mx@wOijF{e@ez@q@ee@pAazGke@qHfAy@~Ke`E{a@iqAuQqH@yBjCm]feGg" +
            "Dx}@gHjj@wOhu@wShh@{b@hs@i{A_lAsFiC}r@}k@w_@c_@oJgCeP_A?dAwkBvU_RrEqeAzNmHlPkHtTuBtT~c@pyAxHj_@tHlRvJna@}@?@jC" +
            "e{@h_Cw`@fmAeL|VkJbL@xI|mAnpA~t@`{@rHzKqF|IgaAfiAukCs{CuF@orBddCaqDxeE}wAxgBulA`rAoyA|}CwmAux@qHBij@poAa_@fs@a" +
            "Nn]qyDxgIg[ju@aaCd}EsdB`eEcn@xqAidEodAie@qKuD@}n@eTsFpEcaAlxAsnBvjCs{@p`Am]xRev@jWecBgjB_Ze_@kgEksEoe@ga@?eAqH" +
            "Bsy@`gAiaB|aCavA~iBs_BxeByaD|zDwjAzoA@tGzbDjuDBdN}vE|hFceAfgA_T_AwDfAsyA_fB?kCs~@yeAwDaAwkCu{Cj{IcqKvSk[t}AglB" +
            "`eAokAdxB{jC~b@_b@ll@iq@tFbA~[~i@~n@da@xDpGv[vKbeClo@`m@hV~n@z\\z[~\\fdAb}@@rGsFfA}@iCdNoPbmCkoCfRhRqFvGgHfh@g_A" +
            "lxA|Jn{@|@?xFjRp`AnnAlg@zi@l_BrnBrFAvw@{dArj@}l@zQ_W`Zte@|sAplBjNtIrFgAAiC|@Av^cxA~`@}n@pHiA~WlT`XtX~UrIxWmA?c" +
            "A~QeLnYgY|SkNzQeYhjAehC`]c~@hL}IfNEjNdCp]IAeAdPsEn[yRnWkh@rDeNlh@aiAvSu_@z@oE}@?}@oEiyAeaAruAsyCpFuG`_@ou@zbAw" +
            "vBrDqEdLg[z@AljBm|D|@?_AyIkg@}\\`_AkrBpsAb_ApHCz@eA?iCz@A|A}sBd}AsfDpWgf@|QoPpj@gq@~b@cd@zoHs~Ipj@qu@viCt{ClJCz" +
            "yAygB?oEu|@yeAyx@g}@_XqVlH{IbjB{iFz@??oEz@AeeAivDjHg[rDuGtn@mLdjCu^bR|@~YzZlbDxeCtFpGxB?hbA|z@rHfC?bAvl@cHnoCi" +
            "{AtYgLtj@q[lJiAnpGle@hc@x@lLdClz@|^jNrIhNjE@bA`uAhMlJCpHgA?cArYgLtYcJvBiCdOkeC|zEra@jRb]l_@IxBqExn@jInkA`Ir]jG" +
            "jx@zDz@cA`QwiCvD?vBkCfOaaCr@}e@")
    };

    /// <summary>BOWI: troncal Terminal Norte - Terminal Sur, 34.935 km, cada 5 min.</summary>
    public static readonly RutaOficial Bowi = new RutaOficial
    {
        Nombre = "BOWI",
        Variante = "Troncal",
        Frecuencia = 5,
        Horario = "T. Norte 5:30 - 22:01 · T. Sur 5:25 - 21:56",
        LongitudKm = 34.935,
        PuntosControl = new List<PuntoControl>
        {
            new PuntoControl("A", "Terminal Norte", 28.705258, -106.124046),
            new PuntoControl("B", "Terminal Sur", 28.619182, -106.019500)
        },
        Trazo = Decodificar(
            "s}_wu@zohliEziAkd@dFCzIdO~Tt{@}WnOqMg[iFgSAcGpBAjTkKvICfF`GvBfS|IfWBfOqbAt|@oBdG@bGdF~BdFCvp@gp@dtAk}A~~AmyAzW" +
            "sSzWu[~^w[hTsWpBAriF_aF|~@u`Ati@_d@htAeqAxWu[n[sSxWw[df@{_@~eA}hAhm@ad@nBcGdFAtIeCpBcCr[kCdF~BvICjM|BzxAtf@|Pz" +
            "B`XGnm@uKfMiGxPmOxWw_@l[uWdtAi}AfMkKvPqWjbA}lAtp@o`AxWu[rfCmaAb_@oKj_Bwl@ff@uStb@qKpBcCxbAe`@ttAmd@l_Ci}@tmB}p" +
            "@pBaC|tCqaAnBcCxmCoaArb@sSzWw[pb@y_@AaC~~@u`AliEm|Exw@ox@l[w_@`fA{hAz~AqiBdm@el@bFcCbFeGpBAAeK`FeGj{AmaBht@kt@" +
            "piB{uBf{D}gE`f@al@`fAyhA`f@cl@zwAeuAdvBrwB|[fc@hXbWhFfSxuArcB`QdWpMfW@`Gq[nKsB@qBaCCeKdMmSniB}yBl[u[h{@whA~p@w" +
            "Ofx@wKzbAaXjTmOr{@c`@|WoO|^_l@zl@wtAze@k`ArPuc@pw@aaBfTu_@plB__Ep~@kuBze@k`AbwBmwEnIo[uBgOCgSqMi[_Uu{@y_@wgA_~" +
            "AuqFsMkc@gFcKwBgOs@crEaYs|BgJ{_AwBiWGo_@bPuhBfPqxA~_AqmKtE_|@hPkpAhBo_@hI}w@vE_|@na@}mDlr@_lHIqg@`FgKCkWsB?sB_" +
            "CcF@qBbCoBbGmbA~xA{aEb}G}WrW{PfCoBbG@bGrB`G`QbOpMf[mBhSqIp[wWxg@wIByIaKsB@AeKlb@cp@fMgG~P~JbU|kAdF`Gjf@oG|bA}S" +
            "lTGxI~BpMf[vBhWwd@tzEmSdeCcWb}Bqd@|nF}OdeClFj_@zBpc@`Jpk@pFts@bJro@t@brEtBfSd\\vcAtX|kAnc@xoAnn@jxBxTlk@h\\zoABf" +
            "O_Mvg@wWzg@_t@~`BcpB~~Dmp@z|Ai[|k@cbAnyBcMnSy^d|@mi@ndAgpA|mCqInWk[v_@yPjKgf@tSsuBmj@kM_Gum@wNmx@uNqf@{ReF@ctB" +
            "hnCuw@thA}eCtvColDaxD_QaOywAjaBem@bl@obCnjC_tFpmGemA`qAcFbCeF@aFdGsIlSamB|uByw@p`AuwEtdF{eFfyFgf@vWk_Brh@ub@rO" +
            "uIfGu{@~Wcx@~[gf@tOq[pOeuEt~AmxCtiAuqFfoBybAf`@o[nOkTpSi{@tdAem@fp@q[nKgTt[aMp[af@bl@oBbGcm@l|@gt@jt@k_A~Smx@u" +
            "Rw_AwZ}P{BoTyBaq@vOspFfmFkt@fl@k{BfzB_fK~xJit@hp@c_@rOob@|c@sb@vWmThCeX{FkM}F_\\ic@iFgW")
    };

}
