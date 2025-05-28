<template>
    <div>
        <my-button></my-button>
    </div>
    <div>
        <tuu name="124443" gender="male" :salery="23" :goods="'123123'" @send-message="send" />
        <tuu name="222" gender="female" :weight="123" @send-message="send" />
        <tuu name="3333" gender="male" :salery="23" @send-message="send" />
        <tuu name="44" gender="female" :weight="123" @send-message="send" />
    </div>
</template>

<script setup lang="ts">
import { Tuu } from '@/components/MyButton/Tuu'
function send(n: unknown) {
    console.log(n)
}
</script>
