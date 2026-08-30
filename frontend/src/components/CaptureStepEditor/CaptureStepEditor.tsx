import type { CaptureActionType, CaptureStepInput } from '../../types/capture';
import './CaptureStepEditor.css';

interface CaptureStepEditorProps {
  steps: CaptureStepInput[];
  onChange: (steps: CaptureStepInput[]) => void;
}

const actionTypes: { value: CaptureActionType; label: string }[] = [
  { value: 'click', label: 'Click' },
  { value: 'fill', label: 'Fill' },
  { value: 'navigate', label: 'Navigate' },
];

function needsSelector(actionType: CaptureActionType): boolean {
  return actionType === 'click' || actionType === 'fill';
}

function needsValue(actionType: CaptureActionType): boolean {
  return actionType === 'fill' || actionType === 'navigate';
}

function valuePlaceholder(actionType: CaptureActionType): string {
  return actionType === 'navigate' ? 'https://example.com/next-page' : 'Text to type';
}

function CaptureStepEditor({ steps, onChange }: CaptureStepEditorProps) {
  function updateStep(index: number, patch: Partial<CaptureStepInput>) {
    onChange(steps.map((step, i) => (i === index ? { ...step, ...patch } : step)));
  }

  function addStep() {
    onChange([...steps, { actionType: 'click', selector: '', value: '' }]);
  }

  function removeStep(index: number) {
    onChange(steps.filter((_, i) => i !== index));
  }

  return (
    <div className="capture-step-editor">
      {steps.length > 0 && (
        <ul className="capture-step-editor__list">
          {steps.map((step, index) => (
            <li key={index} className="capture-step-editor__row">
              <span className="capture-step-editor__order">{index + 1}</span>

              <select
                className="capture-step-editor__select"
                value={step.actionType}
                onChange={(e) => updateStep(index, { actionType: e.target.value as CaptureActionType })}
              >
                {actionTypes.map((type) => (
                  <option key={type.value} value={type.value}>
                    {type.label}
                  </option>
                ))}
              </select>

              {needsSelector(step.actionType) && (
                <input
                  type="text"
                  className="capture-step-editor__input"
                  placeholder="CSS selector, e.g. #submit-button"
                  value={step.selector ?? ''}
                  onChange={(e) => updateStep(index, { selector: e.target.value })}
                />
              )}

              {needsValue(step.actionType) && (
                <input
                  type="text"
                  className="capture-step-editor__input"
                  placeholder={valuePlaceholder(step.actionType)}
                  value={step.value ?? ''}
                  onChange={(e) => updateStep(index, { value: e.target.value })}
                />
              )}

              <button
                type="button"
                className="capture-step-editor__remove"
                onClick={() => removeStep(index)}
                aria-label={`Remove step ${index + 1}`}
              >
                ✕
              </button>
            </li>
          ))}
        </ul>
      )}

      <button type="button" className="capture-step-editor__add" onClick={addStep}>
        + Add step
      </button>
    </div>
  );
}

export default CaptureStepEditor;
