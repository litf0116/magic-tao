/**
 * 图片URL转换工具
 * 将 cdn.molitao.top 转换为 image.molitao.top
 */

/**
 * 转换图片URL
 * @param url 原始URL
 * @returns 转换后的URL
 */
export function convertImageUrl(url: string): string {
  if (!url) return url;

  // 将 cdn.molitao.top 替换为 image.molitao.top
  let result = url.replace(/https?:\/\/cdn\.molitao\.top/g, 'https://image.molitao.top');

  // 将 http://image.molitao.top 替换为 https://image.molitao.top
  result = result.replace(/http:\/\/image\.molitao\.top/g, 'https://image.molitao.top');

  return result;
}

/**
 * 批量转换图片URL数组
 * @param urls URL数组
 * @returns 转换后的URL数组
 */
export function convertImageUrls(urls: string[]): string[] {
  if (!Array.isArray(urls)) return urls;

  return urls.map(url => convertImageUrl(url));
}

/**
 * 转换对象中的图片URL（递归）
 * @param obj 包含URL的对象
 * @param urlKeys 需要转换的URL键名数组
 * @returns 转换后的对象
 */
export function convertObjectImageUrls<T extends Record<string, any>>(
  obj: T,
  urlKeys: string[] = ['avatar', 'url', 'imageUrl', 'image', 'src']
): T {
  if (!obj || typeof obj !== 'object') return obj;

  const result = { ...obj };

  for (const key of urlKeys) {
    if (result[key] && typeof result[key] === 'string') {
      result[key] = convertImageUrl(result[key]);
    }
  }

  return result;
}

/**
 * 转换环境变量中的图片URL
 * @param url 环境变量URL
 * @returns 转换后的URL
 */
export function convertEnvImageUrl(url: string): string {
  return convertImageUrl(url);
}

export default {
  convertImageUrl,
  convertImageUrls,
  convertObjectImageUrls,
  convertEnvImageUrl
};