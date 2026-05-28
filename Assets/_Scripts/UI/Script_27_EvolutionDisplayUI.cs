using MutationSwarm.Evolution;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Panel de información genética de la horda actual.
    /// </summary>
    public class Script_27_EvolutionDisplayUI : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;

        public void UpdateDisplay(Genome dominantSample, int generation, bool superAdaptation)
        {
            if (_uiDocument == null || dominantSample == null)
                return;

            VisualElement root = _uiDocument.rootVisualElement;
            Label geneLabel = root.Q<Label>("DominantGeneLabel");
            Label generationLabel = root.Q<Label>("GenerationLabel");
            VisualElement colorBadge = root.Q<VisualElement>("DominantGeneColor");
            VisualElement alert = root.Q<VisualElement>("SuperAdaptationAlert");

            if (geneLabel != null)
                geneLabel.text = dominantSample.GetDominantGene();
            if (generationLabel != null)
                generationLabel.text = $"Gen. {generation}";
            if (colorBadge != null)
                colorBadge.style.backgroundColor = dominantSample.GetMutationColor();
            if (alert != null)
                alert.style.display = superAdaptation ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
