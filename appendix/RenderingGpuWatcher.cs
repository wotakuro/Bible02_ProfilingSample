using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

#if DEBUG

namespace UTJ
{
    // URP 12での利用を想定しています
    // PlayerSettingsで、define「UNITY_USE_RECORDER」を有効にする必要があります
    // defineは「Tools/RenderingGpuWatcher/AddDefine」の呼出しでも有効化する事が出来ます。
    // 
    // 使い方：上記が整っていれば、ソース入れるだけです

    //----------------------------------------------------------
    // 処理負荷表示用のコンポーネント
    //----------------------------------------------------------
    public class RenderingGpuWatcher : MonoBehaviour
    {
        // UIの更新サイクル（毎フレーム更新だと速すぎるので…)
        private static readonly float UIUpdateCycle = 1.0f;

#if UNITY_EDITOR
        // Defineを追加するメニューです。（大分手抜き）
        static readonly string RecordDefine = "UNITY_USE_RECORDER";
        [UnityEditor.MenuItem("Tools/RenderingGpuWatcher/AddDefine")]
        public static void AddDefine()
        {
            // 必要なプラットフォームを有効にしてください
            AddDefine(UnityEditor.BuildTargetGroup.Standalone);
            AddDefine(UnityEditor.BuildTargetGroup.Android);
            AddDefine(UnityEditor.BuildTargetGroup.iOS);
            AddDefine(UnityEditor.BuildTargetGroup.Switch);
            AddDefine(UnityEditor.BuildTargetGroup.GameCoreXboxOne);
            AddDefine(UnityEditor.BuildTargetGroup.GameCoreXboxSeries);
            AddDefine(UnityEditor.BuildTargetGroup.PS4);
            AddDefine(UnityEditor.BuildTargetGroup.PS5);
        }

        private static void AddDefine(UnityEditor.BuildTargetGroup targetGroup)
        {
            var defineStr = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var defines = defineStr.Split(';');

            foreach (var def in defines)
            {
                if (def == RecordDefine)
                {
                    Debug.Log(targetGroup.ToString() + "には、既にDefine " + RecordDefine + " があります");
                    return;
                }
            }

            if (defineStr.Length > 0)
            {
                defineStr += ";";
            }
            defineStr += RecordDefine;
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineStr.Split(';'));
        }
#endif

        internal ProfilingSamplerGetter getter;
        private static RenderingGpuWatcher instance;
        private static GpuInfoUI gpuInfoUi;
        private float timer;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            if (!instance)
            {
                var gmo = new GameObject("RenderingGpuWatcher", typeof(RenderingGpuWatcher));
                instance = gmo.GetComponent<RenderingGpuWatcher>();
            }
        }

        [ContextMenu("Hide")]
        public static void Hide()
        {
            if (instance) { instance.gameObject.SetActive(false); }
        }

        [ContextMenu("Show")]
        public static void Show()
        {
            if (instance) { instance.gameObject.SetActive(true); }
        }

        private IEnumerator Start()
        {

            if (!SystemInfo.supportsGpuRecorder)
            {
                Debug.LogError("この環境ではGPU Recordに対応していません");
                yield break;
            }
            // EnqueされたPassをゲットするためにあえて少しずらします
            yield return null;
            this.getter = new ProfilingSamplerGetter();
            this.getter.Initialize();
            DontDestroyOnLoad(this.gameObject);
            gpuInfoUi = GpuInfoUI.Create(this.transform, this.getter.allSampler);
        }
        private void Update()
        {
            if (this.getter == null) { return; }
            // Update中に呼ばないと無効になったままのモノがあるみたいなので…
            foreach (var sampler in this.getter.allSampler)
            {
                sampler.enableRecording = true;
            }

            if (timer > 0.0f)
            {
                timer -= Time.deltaTime;
                return;
            }
            gpuInfoUi.Update();
            timer = UIUpdateCycle;
        }
    }
    //----------------------------------------------------------
    // UI部分の処理を行います
    //----------------------------------------------------------
    internal class GpuInfoUI
    {
        private struct ProfilingSampleInfo
        {
            public Text title;
            public Text gpuTime;

            private static Font defaultFont;
            public void Create(Transform parent, ProfilingSampler sampler)
            {
                if (!defaultFont)
                {
                    defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 12);
                }
                var titleObj = new GameObject("tile", typeof(RectTransform), typeof(Text));
                var gpuTimeObj = new GameObject("gpuTime", typeof(RectTransform), typeof(Text));
                titleObj.transform.parent = parent;
                gpuTimeObj.transform.parent = parent;

                title = titleObj.GetComponent<Text>();
                title.font = defaultFont;
                gpuTime = gpuTimeObj.GetComponent<Text>();
                gpuTime.font = defaultFont;
                title.text = sampler.name;
                // Sizeなど
                title.rectTransform.sizeDelta = new Vector2(200, 15);
                gpuTime.rectTransform.sizeDelta = new Vector2(60, 15);
                // AnchorPointの設定等
                SetupAnchorPoint(title.rectTransform);
                SetupAnchorPoint(gpuTime.rectTransform);
            }
            private static void SetupAnchorPoint(RectTransform rectTransform)
            {
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
            }

            public void SetData(ProfilingSampler sampler, int idx)
            {
                title.enabled = true;
                gpuTime.enabled = true;
                title.rectTransform.anchoredPosition = new Vector3(5, -20 * idx - 10, 0);
                gpuTime.rectTransform.anchoredPosition = new Vector3(210, -20 * idx - 10, 0);
                gpuTime.text = sampler.gpuElapsedTime.ToString();
            }
            public void Hide()
            {
                title.enabled = false;
                gpuTime.enabled = false;
            }
        }
        private Canvas canvas;
        private CanvasScaler scaler;
        private Image backGround;
        private List<ProfilingSampleInfo> profilingInfoUIItems;
        private List<ProfilingSampler> profilingSamplers;

        // 
        public static GpuInfoUI Create(Transform parent, List<ProfilingSampler> samplers)
        {
            var obj = new GpuInfoUI(parent, samplers);
            return obj;
        }

        // Create
        private GpuInfoUI(Transform parent, List<ProfilingSampler> samplers)
        {
            this.profilingSamplers = samplers;
            // init canvas
            var canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            this.canvas = canvasObj.GetComponent<Canvas>();
            this.scaler = canvasObj.GetComponent<CanvasScaler>();
            this.canvas.sortingOrder = short.MaxValue;
            this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            this.scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            this.scaler.referenceResolution = new Vector2(800, 600);
            canvasObj.transform.parent = parent;
            // create background
            var backGroundObj = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            backGroundObj.transform.parent = canvasObj.transform;
            backGround = backGroundObj.GetComponent<Image>();
            backGround.color = new Color(0, 0, 0, 0.5f);
            // 配置
            backGround.rectTransform.anchorMin = new Vector2(0, 1);
            backGround.rectTransform.anchorMax = new Vector2(0, 1);
            backGround.rectTransform.pivot = new Vector2(0, 1);

            // 配置参考用
#if false
            {
                // 左上配置
                backGround.rectTransform.anchorMin = new Vector2(0, 1);
                backGround.rectTransform.anchorMax = new Vector2(0, 1);
                backGround.rectTransform.pivot = new Vector2(0, 1);
                // 右上配置
                /*
                backGround.rectTransform.anchorMin = new Vector2(1, 1);
                backGround.rectTransform.anchorMax = new Vector2(1, 1);
                backGround.rectTransform.pivot = new Vector2(1, 1);
                */
                // 左下
                backGround.rectTransform.anchorMin = new Vector2(0, 0);
                backGround.rectTransform.anchorMax = new Vector2(0, 0);
                backGround.rectTransform.pivot = new Vector2(0, 0);
                // 右下
                backGround.rectTransform.anchorMin = new Vector2(1, 0);
                backGround.rectTransform.anchorMax = new Vector2(1, 0);
                backGround.rectTransform.pivot = new Vector2(1, 0);
            }
#endif

            // 座標セット
            backGround.rectTransform.anchoredPosition = new Vector3(0, 0, 0);

            // init samplerinfos
            profilingInfoUIItems = new List<ProfilingSampleInfo>(samplers.Count);
            foreach (var sampler in samplers)
            {
                var infoUI = new ProfilingSampleInfo();
                infoUI.Create(backGround.transform, sampler);
                profilingInfoUIItems.Add(infoUI);
            }
        }
        public void Update()
        {
            int idx = 0;
            for (int i = 0; i < profilingInfoUIItems.Count; ++i)
            {
                if (profilingSamplers[i].gpuSampleCount > 0)
                {
                    profilingInfoUIItems[i].SetData(profilingSamplers[i], idx);
                    ++idx;
                }
                else
                {
                    profilingInfoUIItems[i].Hide();
                }
            }
            // bg
            backGround.rectTransform.sizeDelta = new Vector2(280, idx * 20 + 20);
        }

    }

    //----------------------------------------------------------
    // URPから ProfilingSamplerを取得してくるクラス
    //----------------------------------------------------------
    public class ProfilingSamplerGetter
    {
        public List<ProfilingSampler> allSampler { get; private set; }
        public List<ScriptableRenderPass> allRenderPass { get; private set; }
        public List<ScriptableRendererFeature> allFeatures { get; private set; }

        public void Initialize()
        {
            if (!SystemInfo.supportsGpuRecorder)
            {
                Debug.LogError("この環境ではGPU Recordに対応していません");
                allSampler = new List<ProfilingSampler>();
                return;
            }
            var renderPipelieAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(renderPipelieAsset == null)
            {
                allSampler = new List<ProfilingSampler>();
                Debug.LogError("URPのRenderPipelineAsssetではないようです");
                return;
            }

            this.allRenderPass = new List<ScriptableRenderPass>();
            this.allFeatures = new List<ScriptableRendererFeature>();
            // RendererListで０個目をデフォルトにしないと何故かエラーが出るのでその対策
            try
            {
                var scriptableRenderer = renderPipelieAsset.scriptableRenderer;
                AppendRenderPassInRenderer(allRenderPass, scriptableRenderer);
                AppendRenderFeatures(allFeatures, scriptableRenderer);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
            var scriptableRenderers = GetRenderersByReflection(renderPipelieAsset);
            foreach (var scriptableRenderer in scriptableRenderers)
            {
                if (scriptableRenderer == null) { continue; }
                AppendRenderPassInRenderer(allRenderPass, scriptableRenderer);
                AppendRenderFeatures(allFeatures, scriptableRenderer);
            }


            AppendRenderFeaturesOnMemory(allFeatures);
            AppendScriptableRendererInFeature(allRenderPass, allFeatures);
            // sort pass

            allRenderPass.Sort((p1, p2) =>
            {
                return (int)p1.renderPassEvent - (int)p2.renderPassEvent;
            });

            this.allSampler = new List<ProfilingSampler>();
            AppendExecuteProfilingSampler(allSampler, scriptableRenderers);
            AppendAllProfilingSampler(allSampler, allRenderPass);

            // PassではなくProfilerSample.Get<T>の場合を考慮
            AppendStaticGetSamplers(GetType("UnityEngine.Rendering.Universal.URPProfileId"),
                allSampler);

            // Recordingを有効化
            foreach (var sampler in allSampler)
            {
                sampler.enableRecording = true;
            }
        }

        private static ScriptableRenderer[] GetRenderersByReflection(UniversalRenderPipelineAsset pipelineAsset)
        {

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance;
            var field = pipelineAsset.GetType().GetField("m_Renderers", bindingFlags);
            if (field == null) { return null; }
            var ret = field.GetValue(pipelineAsset) as ScriptableRenderer[];
            return ret;

        }

        private void AppendExecuteProfilingSampler(List<ProfilingSampler> all,ScriptableRenderer[] scriptableRenderers)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static;
            var profilingExecuteProp = typeof(ScriptableRenderer).GetProperty("profilingExecute", bindingFlags);
            if(profilingExecuteProp == null || scriptableRenderers == null)
            {
                return;
            }
            foreach (var scriptableRenderer in scriptableRenderers)
            {
                var sampler = profilingExecuteProp.GetValue(scriptableRenderer) as ProfilingSampler;
                if (!all.Contains(sampler))
                {
                    all.Add(sampler);
                }
            }
        }

        private void AppendAllProfilingSampler(List<ProfilingSampler> all, List<ScriptableRenderPass> allPass)
        {
            foreach (var pass in allPass)
            {
                if (pass == null) { continue; }
                var passType = pass.GetType();
                AppendAllProfilingSamplerBody(all, passType, pass);
            }
        }

        private void AppendAllProfilingSamplerBody(List<ProfilingSampler> all, System.Type type, ScriptableRenderPass pass)
        {
            if (type == null || type == typeof(ScriptableRenderPass)) { return; }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static;
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (field.FieldType.IsAssignableFrom(typeof(ProfilingSampler)))
                {
                    ProfilingSampler sampler = null;
                    if (field.IsStatic)
                    {
                        sampler = field.GetValue(null) as ProfilingSampler;
                    }
                    else
                    {
                        sampler = field.GetValue(pass) as ProfilingSampler;
                    }
                    if (sampler != null && !(all.Contains(sampler)) && ShouldAppendSampler(sampler, all))
                    {
                        //Debug.Log("Pass;" +pass+" Sampler;;" + sampler.name);
                        all.Add(sampler);
                    }
                }
            }
            AppendAllProfilingSamplerBody(all, type.BaseType, pass);
        }


        private bool ShouldAppendSampler(ProfilingSampler sampler, List<ProfilingSampler> all)
        {
            foreach (var src in all)
            {
                if (src.name == sampler.name) { return false; }
            }
            return true;

        }

        private void AppendRenderPassInRenderer(List<ScriptableRenderPass> passes, ScriptableRenderer renderer)
        {
            if (renderer == null) { return; }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var allFields = renderer.GetType().GetFields(bindingFlags);

            foreach (var field in allFields)
            {
                if (typeof(ScriptableRenderPass).IsAssignableFrom(field.FieldType))
                {
                    ScriptableRenderPass pass = field.GetValue(renderer) as ScriptableRenderPass;
                    if (pass != null && !passes.Contains(pass))
                    {
                        passes.Add(pass);
                    }
                }
                // PostProcessPasses対策
                else if (field.FieldType.Name.EndsWith("PostProcessPasses"))
                {
                    AppendPassesInPostProcessPasses(field.GetValue(renderer), passes);
                }
            }
            // EnqueされたPassも追加ですが・・・Enquesして実行直後はカラなのであまり意味がない…
            // AppendEnquedPasses(renderer, passes);
        }

        private static void AppendPassesInPostProcessPasses(System.Object postProcessPassesObj, List<ScriptableRenderPass> passes)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var allFields = postProcessPassesObj.GetType().GetFields(bindingFlags);
            foreach (var field in allFields)
            {
                if (typeof(ScriptableRenderPass).IsAssignableFrom(field.FieldType))
                {
                    ScriptableRenderPass pass = field.GetValue(postProcessPassesObj) as ScriptableRenderPass;
                    if (pass != null)
                    {
                        passes.Add(pass);
                    }
                }
            }
        }

        private static void AppendRenderFeatures(List<ScriptableRendererFeature> destFeatures, ScriptableRenderer renderer)
        {
            if (renderer == null) { return; }
            var src = GetRenderFeatures(renderer);
            if (src == null) { return; }
            foreach (var feature in src)
            {
                if (!destFeatures.Contains(feature))
                {
                    destFeatures.Add(feature);
                }
            }
        }

        private static List<ScriptableRendererFeature> GetRenderFeatures(ScriptableRenderer renderer)
        {
            var type = renderer.GetType();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var propInfo = type.GetProperty("rendererFeatures", bindingFlags);
            if (propInfo == null) { return null; }

            // List<ScriptableRendererFeature> rendererFeatures
            //            renderer.supportedRenderingFeatures
            return propInfo.GetValue(renderer) as List<ScriptableRendererFeature>;
        }

        private static void AppendRenderFeaturesOnMemory(List<ScriptableRendererFeature> features)
        {
            var allFeatures = Resources.FindObjectsOfTypeAll<ScriptableRendererFeature>();
            foreach (var feature in allFeatures)
            {
                if (!features.Contains(feature))
                {
                    features.Add(feature);
                }
            }
        }

        private static void AppendScriptableRendererInFeature(List<ScriptableRenderPass> passes, List<ScriptableRendererFeature> features)
        {
            if (features == null) { return; }
            foreach (var feature in features)
            {
                var type = feature.GetType();
                AppendScriptableRendererInFeatureBody(passes, type, feature);
            }
        }

        private static void AppendScriptableRendererInFeatureBody(List<ScriptableRenderPass> passes, System.Type type, ScriptableRendererFeature feature)
        {
            if (type == null || type == typeof(ScriptableRendererFeature)) { return; }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (!(typeof(ScriptableRenderPass).IsAssignableFrom(field.FieldType)))
                {
                    continue;
                }
                var pass = field.GetValue(feature) as ScriptableRenderPass;
                if (pass != null && !passes.Contains(pass))
                {
                    passes.Add(pass);
                }
            }
            AppendScriptableRendererInFeatureBody(passes, type.BaseType, feature);
        }

        private static void AppendEnquedPasses(ScriptableRenderer renderer, List<ScriptableRenderPass> passes)
        {
            var type = renderer.GetType();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var propInfo = type.GetProperty("activeRenderPassQueue", bindingFlags);
            if (propInfo == null) { return; }
            var activePass = propInfo.GetValue(renderer) as List<ScriptableRenderPass>;
            if (activePass == null) { return; }
            foreach (var pass in activePass)
            {
                if (pass != null && !passes.Contains(pass))
                {
                    passes.Add(pass);
                }
            }
        }



        private static void AppendStaticGetSamplers(System.Type type, List<ProfilingSampler> sampleres)
        {

            var vals = type.GetEnumValues();
            var method = GetSamplerGetMethod();
            if (method == null)
            {
                Debug.Log("Get Method Not Found");
                return;
            }
            var genericMethod = method.MakeGenericMethod(type);

            foreach (var val in vals)
            {
                var sampler = genericMethod.Invoke(null, new object[] { val }) as ProfilingSampler;
                if (!sampleres.Contains(sampler))
                {
                    sampleres.Add(sampler);
                }
            }
        }

        private static MethodInfo GetSamplerGetMethod()
        {
            BindingFlags binding = BindingFlags.Static | BindingFlags.Public;
            var methods = typeof(ProfilingSampler).GetMethods(binding);
            foreach (var method in methods)
            {
                if (method.Name != "Get") { continue; }
                var types = method.GetGenericArguments();
                if (types.Length >= 1 && types[0].IsEnum)
                {
                    return method;
                }
            }
            return null;
        }



        private static System.Type GetType(string fullname)
        {
            var appDomain = System.AppDomain.CurrentDomain;
            foreach (var asm in appDomain.GetAssemblies())
            {
                var type = asm.GetType(fullname);
                if (type != null) { return type; }
            }
            return null;
        }
    }


#if UNITY_EDITOR
    //----------------------------------------------------------
    // for Debug
    //----------------------------------------------------------
    [UnityEditor.CustomEditor(typeof(RenderingGpuWatcher))]
    public class RenderingGpuWatcherEditor : UnityEditor.Editor
    {
        private List<string> passes = new List<string>();
        private List<string> features = new List<string>();
        private List<string> samplers = new List<string>();

        private bool samplesFold = false;
        private bool featuresFold = false;
        private bool passesFold = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            RenderingGpuWatcher wathecr = this.target as RenderingGpuWatcher;
            passes.Clear();
            foreach (var pass in wathecr.getter.allRenderPass)
            {
                if (pass == null) { continue; }
                var str = pass.ToString();
                if (!this.passes.Contains(str))
                {
                    this.passes.Add(str);
                }
            }
            passes.Sort();

            samplers.Clear();
            foreach (var sampler in wathecr.getter.allSampler)
            {
                if (sampler == null) { continue; }
                var str = sampler.name;
                if (!this.samplers.Contains(str))
                {
                    this.samplers.Add(str);
                }
            }
            samplers.Sort();

            features.Clear();
            foreach (var feature in wathecr.getter.allFeatures)
            {
                if (feature == null) { continue; }
                var str = feature.name + "  (" + feature.GetType() + ")";
                if (!this.features.Contains(str))
                {
                    this.features.Add(str);
                }
            }
            features.Sort();

            featuresFold = UnityEditor.EditorGUILayout.Foldout(featuresFold, "Features ");
            if (featuresFold)
            {
                foreach (var feature in features)
                {
                    UnityEditor.EditorGUILayout.LabelField("    " + feature);
                }
            }
            passesFold = UnityEditor.EditorGUILayout.Foldout(passesFold, "Pass ");
            if (passesFold)
            {
                foreach (var pass in passes)
                {
                    UnityEditor.EditorGUILayout.LabelField("    " + pass);
                }
            }
            samplesFold = UnityEditor.EditorGUILayout.Foldout(samplesFold, "ProfilerSamplers");
            if (samplesFold)
            {
                foreach (var sampler in samplers)
                {
                    UnityEditor.EditorGUILayout.LabelField("    " + sampler);
                }
            }
        }
    }
#endif
}
#endif