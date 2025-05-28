import type { FunctionalComponent } from 'vue'
type Props = {
    name: string
} & (MaleProp | FemaleProp)

type MaleProp = { gender: 'male'; salery: number; goods?: string }
type FemaleProp = {
    gender: 'female'
    weight: number
}

type Events = {
    sendMessage(message: string): void
}

export const Tuu: FunctionalComponent<Props, Events> = (props, context) => {
    return (
        <div
            class={`m-4 border border-solid border-amber rounded-lg shadow p-4 text-red-500 text-[50px] cursor-pointer`}
            onClick={() => context.emit('sendMessage', props.name)}
        >
            <div>name:{props.name}</div>
            {props.gender === 'male' ? (
                <>
                    <div>
                        <span class="i-logos-vue size-20"></span> {props.gender}
                    </div>
                    <div>
                        <span class="i-mdi-cog size-20"></span> {props.salery}
                    </div>
                </>
            ) : (
                <>
                    <div class="size-20 i-logos-youtube"></div> {props.gender} {props.weight}
                </>
            )}
        </div>
    )
}
