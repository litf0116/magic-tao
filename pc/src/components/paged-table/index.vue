<template>
	<div class="flex flex-col items-center sm:flex-row sm:items-end xl:items-start">
		<form v-if="showSearch" class="xl:flex sm:mr-auto w-full">
			<div class="sm:flex items-center sm:mr-4 mt-2 xl:mt-0">
				<slot name="search">
					<el-input v-model="queryForm.keyword" type="text" class="form-control w-72 mt-2 sm:mt-0"
						:placeholder="searchText" />
				</slot>
			</div>
			<div class="mt-2 xl:mt-0">
				<el-button type="primary" class="w-full sm:w-16" @click="handleSearch">搜索</el-button>
				<el-button class="w-full sm:w-16 mt-2 sm:mt-0 sm:ml-1" @click="handleResetSearch">重置</el-button>
			</div>
		</form>
	</div>
	<div class="flex items-center my-4">
		<div class="flex-1">
			<slot name="filter"></slot>
		</div>
		<div class="flex mt-5 sm:mt-0">
			<slot name="btns"></slot>
			<template v-if="showExport">
				<!-- <button class="btn btn-outline-secondary w-1/2 sm:w-auto mr-2">
                 <PrinterIcon class="w-4 h-4 mr-2" /> 打印
          </button>-->
				<div class="dropdown w-1/2 sm:w-auto">
					<button class="dropdown-toggle btn btn-outline-secondary w-full sm:w-auto" aria-expanded="false">
						<!-- <FileTextIcon class="w-4 h-4 mr-2" /> -->
						查询结果导出
						<!-- <ChevronDownIcon class="w-4 h-4 ml-auto sm:ml-2" /> -->
					</button>
					<div class="dropdown-menu w-48">
						<div v-if="table.totalCount" class="dropdown-menu__content box dark:bg-dark-1 p-2">
							<a href="javascript:;"
								class="flex items-center p-2 transition duration-300 ease-in-out bg-white dark:bg-dark-1 hover:bg-gray-200 dark:hover:bg-dark-2 rounded-md"
								@click="exportExcel">
								<!-- <FileTextIcon class="w-4 h-4 mr-2" /> -->
								导出 XLSX
							</a>
							<slot name="otherExport"></slot>
						</div>
					</div>
				</div>
			</template>
		</div>
	</div>

	<div class="overflow-x-auto scrollbar-hidden mt-4">
		<div v-if="showTopPager && table.totalCount" class="mb-2">
			<el-pagination v-model:currentPage="table.page" :page-sizes="pageSizes" :page-size="table.pageSize"
				layout="sizes, prev, pager, next, jumper" :total="table.totalCount" @size-change="handleSizeChange"
				@current-change="handleCurrentChange" />
		</div>
		<el-table :size="size" ref="tableRef" v-loading="table.listLoading" :data="tableItems"
			element-loading-text="loading..." border stripe fit highlight-current-row :tree-props="treeProps"
			:default-expand-all="defaultExpandAll" :row-key="rowKey" @sort-change="sort"
			@selection-change="handleSelectionChange" @cellClick="cellClick">
			<slot></slot>
		</el-table>
		<div v-if="showPager && table.totalCount" class="mt-2">
			<el-pagination v-model:currentPage="table.page" :page-sizes="pageSizes" :page-size="table.pageSize"
				layout="sizes, prev, pager, next, jumper" :total="table.totalCount" @size-change="handleSizeChange"
				@current-change="handleCurrentChange" />
		</div>
	</div>
	<el-dialog title="文件下载" v-model="dialogVisible" width="250px" destroy-on-close append-to-body>
		<div class="flex items-center justify-between"></div>
		<div class="mb-4 text-green-500 text-lg flex items-center justify-center">
			<span class="size-6 i-mdi:download mr-2"></span>导出成功
		</div>
		<a class="block" :href="exportUrl" target="_blank">点击下载</a>
	</el-dialog>
</template>

<script lang="ts">
import { createTableTree } from '@/utils/tree'
import { DefaultRow, TableProps } from 'element-plus/es/components/table/src/table/defaults'
import { computed, defineComponent, reactive, toRefs, PropType, onMounted } from 'vue'

export default defineComponent({
	props: {
		fetchFunction: Function,
		exportExcelFunction: Function,
		cellClick: Function,
		size: { type: String, default: 'default' },
		pageSize: {
			type: Number,
			default: 10,
		},
		pageSizes: {
			type: Array as PropType<number[]>,
			default: () => [10, 20, 50, 100],
		},
		showExport: {
			type: Boolean,
			default: false,
		},
		showPager: {
			type: Boolean,
			default: true,
		},
		showTopPager: {
			type: Boolean,
			default: false,
		},
		showSearch: {
			type: Boolean,
			default: true,
		},
		buttonSize: {
			type: String,
			default: 'default',
		},
		rowKey: [String, Function] as PropType<TableProps<DefaultRow>['rowKey']>,
		defaultExpandAll: Boolean,
		treeProps: {
			type: Object as PropType<TableProps<DefaultRow>['treeProps']>,
			default: () => {
				return {
					hasChildren: 'hasChildren',
					children: 'children',
				}
			},
		},
		tableSort: {
			type: String,
			default: '',
		},
		queryStatus: {
			type: [String, Number, Boolean],
			default: '',
		},
		queryExtPath: {
			type: String,
			default: '',
		},
		queryInclude: {
			type: String,
			default: undefined,
		},
		searchText: {
			type: String,
			default: '输入搜索内容...',
		},
		autoFetch: {
			type: Boolean,
			default: true,
		},
		isCalculationPaging: {
			type: Boolean,
			default: true,
		}
	},
	emits: ['clearSearch', 'afterFetch'],
	setup(props, { emit }) {
		// Export
		const onExportXlsx = () => {
			console.log('onExportXlsx')
		}
		// Print
		const onPrint = () => {
			console.log('onPrint')
		}

		onMounted(() => {
			console.log('PagedTable onMounted')
			console.log(data)
			if (props.autoFetch) fetchData()
		})

		const fetchData = async () => {
			if (props.fetchFunction) {
				data.table.listLoading = true
				await props.fetchFunction!({
					// keyword: data.queryForm.keyword,
					// status: data.queryForm.status,
					// extPath: data.queryForm.extPath,
					...data.queryForm,

					include: props.queryInclude,
					// isActive: data.queryForm.isActive,
					sorting: data.table.sorting,
					skipCount: skipCount.value,
					maxResultCount: data.table.pageSize,
					// organizationUnitId: data.queryForm.organizationUnitId,
				}).then(async (res: any) => {
					if (res.data) {
						data.table.totalCount = res.data.totalCount
						data.tableItems = res.data.items
						emit('afterFetch', res.data)
					} else {
						console.log(res)
						data.table.totalCount = res.totalCount
						if (props.rowKey) {
							const tree = createTableTree(res.items, 'pid', 'id', null, 'children')
							console.log(tree)
							data.tableItems = tree
						} else {
							data.tableItems = res.items
						}

						emit('afterFetch', res)
					}
				})
				data.table.listLoading = false
			}
		}

		const skipCount = computed(() => {
			if (props.isCalculationPaging) {
				return (data.table.page - 1) * data.table.pageSize
			} else {
				return data.table.page
			}
		})

		const clearSelection = () => {
			tableRef.value!.clearSelection()
		}

		const tableRef = ref(null as any)

		const data = reactive({
			queryForm: {
				keyword: '',
				status: props.queryStatus,
				extPath: props.queryExtPath,
				include: props.queryInclude,
				// organizationUnitId: '',
			} as any,
			tableItems: [] as any[],
			table: {
				listLoading: false,
				page: 1,
				totalCount: 0,
				pageSize: props.pageSize,
				sorting: props.tableSort,
			} as any,
			selection: [] as any[],
			handleSearch() {
				fetchData()
			},
			handleResetSearch() {
				data.table.page = 1
				data.queryForm = {
					keyword: '',
					status: props.queryStatus,
					extPath: props.queryExtPath,
					include: props.queryInclude,
					organizationUnitId: '',
				}
				data.table.sorting = ''
				emit('clearSearch')
				fetchData()
			},
			handleSizeChange: (_: number) => {
				data.table.page = 1
				data.table.pageSize = _
				fetchData()
			},
			handleCurrentChange: (_: number) => {
				data.table.page = _
				fetchData()
			},
			handleSelectionChange: (_: any) => {
				console.log(_)
				data.selection = _
			},
			sort: (_: any) => {
				// console.log('sort : ', _)
				if (_.prop && _.order) {
					data.table.sorting = `${_.prop} ${_.order}`
				}
				fetchData()
			},

			dialogVisible: false,
			exportUrl: '',
			exportExcel: async () => {
				if (props.exportExcelFunction) {
					data.table.listLoading = true
					await props.exportExcelFunction!({
						...data.queryForm,

						include: props.queryInclude,
						// isActive: data.queryForm.isActive,
						sorting: data.table.sorting,
						skipCount: skipCount.value,
						maxResultCount: data.table.pageSize,
						// organizationUnitId: data.queryForm.organizationUnitId,
					}).then(
						async (res: any) => {
							data.dialogVisible = true
							data.exportUrl = res
							data.table.listLoading = false
						},
						() => {
							data.table.listLoading = false
						}
					)
				}
			},
		})

		const params = computed(() => {
			return {
				...data.queryForm,
				include: props.queryInclude,
				// isActive: data.queryForm.isActive,
				sorting: data.table.sorting,
				skipCount: skipCount.value,
				maxResultCount: data.table.pageSize,
			}
		})

		return {
			tableRef,
			onExportXlsx,
			onPrint,
			fetchData,
			clearSelection,
			...toRefs(data),
			...toRefs(props),
			params,
		}
	},
})
</script>
