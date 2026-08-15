import { createFormHook, createFormHookContexts } from "@tanstack/solid-form";

export type FormProps = {
  label: string;
  type: string;
  inputClass: string;
  labelClass: string;
  errorClass: string;
};

export const { fieldContext, formContext, useFieldContext } = createFormHookContexts();
export const { useAppForm } = createFormHook({
  fieldContext,
  formContext,
  fieldComponents: {
    TextField,
  },
  formComponents: {},
});

export default function TextField(props: FormProps) {
  const field = useFieldContext<string>();
  return (
    <div>
      <label class={props.labelClass}>{props.label}</label>
      <input
        class={props.inputClass}
        type={props.type}
        onInput={(e) => field().handleChange(e.target.value)}
        onBlur={field().handleBlur}
        value={field().state.value}
      />
      {!field().state.meta.isValid
        ? field().state.meta.errors.map((e) => (
            <>
              <em class={props.errorClass} role="alert">
                {e?.message}
              </em>
              <br />
            </>
          ))
        : null}
    </div>
  );
}
