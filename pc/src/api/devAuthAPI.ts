import request from '@/utils/request'

/**
 * 为指定用户生成token（仅限开发环境使用）
 * @param userId 用户ID
 * @returns token信息
 */
export function generateTokenForUser(userId: number) {
  return request({
    url: '/api/TokenAuth/GenerateTokenForUser',
    method: 'post',
    data: {
      userId
    }
  })
}