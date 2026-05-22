interface ValidationError {
    message: string
    members: string[]
}

// vue-router RouteMeta 类型扩展
import 'vue-router'

declare module 'vue-router' {
    interface RouteMeta {
        title?: string
        hidden?: boolean
        icon?: string
        permissions?: string[]
        roles?: string[]
    }
}
