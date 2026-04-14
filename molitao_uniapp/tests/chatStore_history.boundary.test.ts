import { describe, it, expect } from 'vitest'
import { mergeHistoryForChannel } from '../src/stores/chatStore'

describe('mergeHistoryForChannel boundary tests', () => {
  it('merges and sorts by time when both new and existing items present', () => {
    const newItems: any[] = [ { id: 101, time: 5 }, { id: 102, time: 2 } ]
    const existing: any[] = [ { id: 1, time: 3 } ]
    const res = mergeHistoryForChannel('-10_announcement', newItems as any, existing as any, false)
    expect(res.length).toBe(3)
    expect(res[0].time).toBe(2)
    expect(res[1].time).toBe(3)
    expect(res[2].time).toBe(5)
  })
})
