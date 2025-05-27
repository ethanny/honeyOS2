using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoneyOS.Diagnostics
{
    /// <summary>
    /// Diagnostic tool to identify and fix layout errors without deleting files
    /// </summary>
    public class LayoutDiagnostic : MonoBehaviour
    {
        [Header("Diagnostic Results")]
        [TextArea(10, 20)]
        public string diagnosticResults = "Click 'Run Full Diagnostic' to analyze layout issues";

        [Header("Auto-Fix Options")]
        public bool autoFixMissingRectTransforms = true;
        public bool autoFixDuplicateLayoutGroups = true;
        public bool autoFixMissingReferences = true;

        private List<string> issues = new List<string>();
        private List<string> fixes = new List<string>();

        [ContextMenu("Run Full Diagnostic")]
        public void RunFullDiagnostic()
        {
            issues.Clear();
            fixes.Clear();
            
            Debug.Log("=== LAYOUT DIAGNOSTIC STARTED ===");
            
            CheckForLayoutGroupIssues();
            CheckForRectTransformIssues();
            CheckForMissingReferences();
            CheckForConflictingComponents();
            CheckUITabsSpecific();
            CheckRoundedCornersSpecific();
            
            GenerateReport();
            
            Debug.Log("=== LAYOUT DIAGNOSTIC COMPLETED ===");
        }

        private void CheckForLayoutGroupIssues()
        {
            var layoutGroups = FindObjectsOfType<LayoutGroup>();
            
            foreach (var layoutGroup in layoutGroups)
            {
                // Check if LayoutGroup has RectTransform
                var rectTransform = layoutGroup.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    issues.Add($"ERROR: {layoutGroup.gameObject.name} has LayoutGroup but no RectTransform!");
                    
                    if (autoFixMissingRectTransforms)
                    {
                        // This would require destroying and recreating the GameObject
                        fixes.Add($"MANUAL FIX REQUIRED: Convert {layoutGroup.gameObject.name} to UI GameObject");
                    }
                }

                // Check for multiple layout groups on same object
                var allLayoutGroups = layoutGroup.GetComponents<LayoutGroup>();
                if (allLayoutGroups.Length > 1)
                {
                    issues.Add($"WARNING: {layoutGroup.gameObject.name} has {allLayoutGroups.Length} LayoutGroups!");
                    
                    if (autoFixDuplicateLayoutGroups)
                    {
                        // Keep only the first one
                        for (int i = 1; i < allLayoutGroups.Length; i++)
                        {
                            if (Application.isPlaying)
                            {
                                Destroy(allLayoutGroups[i]);
                            }
                            else
                            {
                                DestroyImmediate(allLayoutGroups[i]);
                            }
                        }
                        fixes.Add($"FIXED: Removed duplicate LayoutGroups from {layoutGroup.gameObject.name}");
                    }
                }

                // Check for conflicting layout components
                var contentSizeFitter = layoutGroup.GetComponent<ContentSizeFitter>();
                if (contentSizeFitter != null)
                {
                    issues.Add($"INFO: {layoutGroup.gameObject.name} has both LayoutGroup and ContentSizeFitter (may conflict)");
                }
            }
        }

        private void CheckForRectTransformIssues()
        {
            var allTransforms = FindObjectsOfType<Transform>();
            
            foreach (var transform in allTransforms)
            {
                // Check if UI components are on non-UI GameObjects
                var uiComponents = transform.GetComponents<Graphic>();
                var layoutGroups = transform.GetComponents<LayoutGroup>();
                var contentSizeFitters = transform.GetComponents<ContentSizeFitter>();
                
                bool hasUIComponents = uiComponents.Length > 0 || layoutGroups.Length > 0 || contentSizeFitters.Length > 0;
                bool hasRectTransform = transform is RectTransform;
                
                if (hasUIComponents && !hasRectTransform)
                {
                    issues.Add($"ERROR: {transform.gameObject.name} has UI components but no RectTransform!");
                    fixes.Add($"MANUAL FIX: Convert {transform.gameObject.name} to UI GameObject (GameObject → UI → Panel, then remove Panel component)");
                }
            }
        }

        private void CheckForMissingReferences()
        {
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var mono in allMonoBehaviours)
            {
                if (mono == null)
                {
                    issues.Add("ERROR: Found null MonoBehaviour reference!");
                    continue;
                }

                // Check for missing script references (Editor only)
                #if UNITY_EDITOR
                try
                {
                    var serializedObject = new SerializedObject(mono);
                    var property = serializedObject.GetIterator();
                    
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (property.objectReferenceValue == null && !string.IsNullOrEmpty(property.objectReferenceInstanceIDValue.ToString()) && property.objectReferenceInstanceIDValue != 0)
                            {
                                issues.Add($"WARNING: {mono.gameObject.name}.{mono.GetType().Name} has missing reference: {property.name}");
                                
                                if (autoFixMissingReferences)
                                {
                                    // Clear the missing reference
                                    property.objectReferenceValue = null;
                                    serializedObject.ApplyModifiedProperties();
                                    fixes.Add($"FIXED: Cleared missing reference {property.name} from {mono.gameObject.name}");
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // Skip objects that can't be serialized
                    Debug.LogWarning($"Could not check references for {mono.gameObject.name}: {ex.Message}");
                }
                #else
                // In build, just check for null components
                if (mono == null)
                {
                    issues.Add($"WARNING: Found null component reference");
                }
                #endif
            }
        }

        private void CheckForConflictingComponents()
        {
            // Check for Canvas issues
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                var canvasScaler = canvas.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    issues.Add($"INFO: Canvas {canvas.gameObject.name} has no CanvasScaler");
                }

                var graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                {
                    issues.Add($"INFO: Canvas {canvas.gameObject.name} has no GraphicRaycaster");
                }
            }
        }

        private void CheckUITabsSpecific()
        {
            // Check for UITabs specific issues
            var tabsComponents = FindObjectsOfType<MonoBehaviour>();
            foreach (var component in tabsComponents)
            {
                if (component.GetType().Name.Contains("TabsUI"))
                {
                    // Check if the tabs component has proper setup
                    var rectTransform = component.GetComponent<RectTransform>();
                    if (rectTransform == null)
                    {
                        issues.Add($"ERROR: UITabs component {component.gameObject.name} needs RectTransform!");
                    }
                }
            }
        }

        private void CheckRoundedCornersSpecific()
        {
            // Check for Rounded Corners specific issues
            var roundedCornerComponents = FindObjectsOfType<MonoBehaviour>();
            foreach (var component in roundedCornerComponents)
            {
                if (component.GetType().Name.Contains("RoundedCorners") || component.GetType().Name.Contains("ImageWith"))
                {
                    var rectTransform = component.GetComponent<RectTransform>();
                    if (rectTransform == null)
                    {
                        issues.Add($"ERROR: Rounded Corners component {component.gameObject.name} needs RectTransform!");
                    }

                    var image = component.GetComponent<Image>();
                    if (image == null)
                    {
                        issues.Add($"WARNING: Rounded Corners component {component.gameObject.name} has no Image component!");
                    }
                }
            }
        }

        private void GenerateReport()
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== LAYOUT DIAGNOSTIC REPORT ===");
            report.AppendLine($"Scan completed at: {System.DateTime.Now}");
            report.AppendLine($"Total issues found: {issues.Count}");
            report.AppendLine($"Auto-fixes applied: {fixes.Count}");
            report.AppendLine();

            if (issues.Count == 0)
            {
                report.AppendLine("✅ NO LAYOUT ISSUES FOUND!");
                report.AppendLine("Your project's UI layout is healthy.");
            }
            else
            {
                report.AppendLine("🔍 ISSUES FOUND:");
                foreach (var issue in issues)
                {
                    report.AppendLine($"  • {issue}");
                }
            }

            if (fixes.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("🔧 FIXES APPLIED:");
                foreach (var fix in fixes)
                {
                    report.AppendLine($"  • {fix}");
                }
            }

            report.AppendLine();
            report.AppendLine("=== RECOMMENDATIONS ===");
            
            if (issues.Count > 0)
            {
                report.AppendLine("1. Address ERROR items first (these break functionality)");
                report.AppendLine("2. Review WARNING items (these may cause issues)");
                report.AppendLine("3. Consider INFO items for optimization");
                report.AppendLine();
                report.AppendLine("For manual fixes, see LAYOUT_ERROR_SOLUTIONS.md");
            }
            else
            {
                report.AppendLine("Your layout is healthy! If you're still seeing errors:");
                report.AppendLine("1. Check Unity Console for specific error messages");
                report.AppendLine("2. The issue might be in scene-specific objects");
                report.AppendLine("3. Try running this diagnostic in Play mode");
            }

            diagnosticResults = report.ToString();
            
            Debug.Log(diagnosticResults);
        }

        [ContextMenu("Fix Common Issues")]
        public void FixCommonIssues()
        {
            autoFixMissingRectTransforms = true;
            autoFixDuplicateLayoutGroups = true;
            autoFixMissingReferences = true;
            
            RunFullDiagnostic();
        }

        [ContextMenu("Clear Diagnostic Results")]
        public void ClearResults()
        {
            diagnosticResults = "Results cleared. Run diagnostic again.";
            issues.Clear();
            fixes.Clear();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LayoutDiagnostic))]
    public class LayoutDiagnosticEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            
            var diagnostic = (LayoutDiagnostic)target;
            
            if (GUILayout.Button("Run Full Diagnostic", GUILayout.Height(30)))
            {
                diagnostic.RunFullDiagnostic();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fix Common Issues"))
            {
                diagnostic.FixCommonIssues();
            }
            if (GUILayout.Button("Clear Results"))
            {
                diagnostic.ClearResults();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
} 