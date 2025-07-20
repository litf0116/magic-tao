import { AuditNodeCreateOrEditDto } from '@/api/appService'
import * as _ from 'lodash'

export interface INodesBlock {
    items: AuditNodeCreateOrEditDto[]
}

export function createNodes(array: AuditNodeCreateOrEditDto[]) {
    if (!array) return []
    const result: INodesBlock[] = []
    for (let index = 0; index < array.length; index++) {
        const element = array[index]
        if (!result[element.index!]) {
            result[element.index!] = { items: [] }
        }
        result[element.index!].items.push(element)
    }
    return result
}

export function flatenNodes(array: INodesBlock[]) {
    const result: AuditNodeCreateOrEditDto[] = []
    array = array.filter((x) => x.items.length > 0)
    for (let i = 0; i < array.length; i++) {
        array[i].items.forEach((x) => {
            result.push({ ...x, index: i })
        })
    }
    return result
}

export function createTableTree(
    array: any[],
    parentIdProperty: string,
    idProperty: string,
    parentIdValue: any,
    childrenProperty: string
) {
    const tree: any[] = []
    const nodes = _.filter(array, [parentIdProperty, parentIdValue])
    _.forEach(nodes, (node: any) => {
        const newNode = Object.assign({}, node, {
            hasChildren: false,
            isLeaf: true,
        })
        const _children = createTableTree(array, parentIdProperty, idProperty, node[idProperty], childrenProperty)

        if (_children.length > 0) {
            newNode[childrenProperty] = _children
            newNode['hasChildren'] = true
        }
        if (_children.length > 0) newNode['isLeaf'] = false
        tree.push(newNode)
    })
    return tree
}

export function createTree(
    array: any,
    parentIdProperty: string,
    idProperty: string,
    parentIdValue: any,
    childrenProperty: string,
    disabled: any,
    path: number[] = [],
    labelProperty: string
) {
    const tree: any[] = []

    const nodes = _.filter(array, [parentIdProperty, parentIdValue])
    _.forEach(nodes, (node: any) => {
        const newNode: any = {
            label: `${node[labelProperty]}`,
            data: node,
            value: node[idProperty],
            id: node[idProperty],
            expanded: true,
            disabled,
            path: [...path, node[idProperty]],
        }

        const _children: any = createTree(
            array,
            parentIdProperty,
            idProperty,
            node[idProperty],
            childrenProperty,
            disabled,
            newNode.path,
            labelProperty
        )
        newNode[childrenProperty] = _children
        if (_children.length <= 0) delete newNode[childrenProperty]
        tree.push(newNode)
    })
    return tree
}

export function createOUTree(
    array: any,
    parentIdProperty: any,
    idProperty: any,
    parentIdValue: any,
    childrenProperty: any,
    disabled: any,
    path: number[] = []
) {
    const tree: any[] = []

    const nodes = _.filter(array, [parentIdProperty, parentIdValue])
    _.forEach(nodes, (node: any) => {
        const newNode: any = {
            label: `${node.displayName}`,
            data: node,
            value: node[idProperty],
            id: node[idProperty],
            expanded: true,
            disabled,
            path: [...path, node[idProperty]],
        }

        const _children: any = createOUTree(
            array,
            parentIdProperty,
            idProperty,
            node[idProperty],
            childrenProperty,
            disabled,
            newNode.path
        )
        newNode[childrenProperty] = _children
        if (_children.length <= 0) delete newNode[childrenProperty]
        tree.push(newNode)
    })
    return tree
}

export function treeFind(tree: any[], func: Function) {
    for (const data of tree) {
        if (func(data)) return data
        if (data.children && data.children.length > 0) {
            const res: any = treeFind(data.children, func)
            if (res) return res
        }
    }
    return null
}

export function getKeys(array: any[]) {
    if (array) {
        return Array.from(new Set(['Pages', ..._getKeys(array), ...array]))
    } // 去重
    return []
}

function _getKeys(array: any[]) {
    let _result: any[] = []
    ;[...array].forEach((z) => {
        const _index = z.lastIndexOf('.')
        if (_index > 0) {
            const _p = z.substring(0, _index)
            _result = [..._result, _p]
            _result = [..._result, ..._getKeys(_p)]
        }
    })
    return _result
}

export function rebuildKeys(array: any[]) {
    _.remove(array, (node: any) => {
        const _l = array.filter((zz) => zz.indexOf(node) > -1).length
        return _l > 1
    })
    return array
}
