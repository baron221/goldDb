<template>
<div class="order-list-luxury">
    <template v-if="groupedOrders.length > 0">
      <div v-for="group in groupedOrders" :key="group.key" class="order-group-container">

        <div class="order-group-header-luxury" :class="{ 'group-header-compact': isMobile }">
          <div class="g-info-left">
            <span class="group-icon-wrapper">
              <el-icon v-if="group.groupOrderNo"><Shop /></el-icon>
              <el-icon v-else><OfficeBuilding /></el-icon>
            </span>
            <div class="group-title-meta">
              <h3 class="group-no">
                {{ group.groupOrderNo ? `묶음주문: ${group.groupOrderNo}` : `개별주문: ${group.orders[0].orderNo}` }}
              </h3>
              <span class="group-date">{{ parseTime(group.createdAt, '{y}-{m}-{d} {h}:{i}') }}</span>
            </div>
          </div>
          <div class="g-info-right">
            <div class="group-total-info">
              <span class="label">총 주문금액</span>
              <span class="amount">{{ formatPrice(group.totalAmount) }} <span class="unit">원</span></span>
            </div>
          </div>
        </div>

        <div v-for="order in group.orders" :key="order.id" class="order-luxury-card">
          <div class="card-top-header">
            <div class="left-meta">
              <el-tag :type="getStatusType(order.status)" effect="dark" class="status-indicator-tag">
                {{ getStatusLabel(order.status) }}
              </el-tag>
              <div class="order-id-info">
                <span class="order-no">{{ order.orderNo }}</span>
                <span class="user-company-badge">
                  <i class="fas fa-store"></i> {{ order.companyName || 'Unknown' }}
                </span>
              </div>
            </div>
            <div class="right-actions">

              <el-button
                v-if="order.status === 'PENDING' || order.status === '정산대기'"
                type="warning"
                class="action-pill-btn"
                @click.stop="emit('settle', order)"
              >
                정산확인요청
              </el-button>

              <el-button
                v-if="order.status === 'ORDERED' || order.status === '접수대기'"
                type="danger"
                class="action-pill-btn"
                @click.stop="emit('cancel', order)"
              >
                주문취소
              </el-button>

              <el-button
                v-if="isStatementVisible(order.status)"
                type="info"
                class="action-pill-btn"
                @click.stop="emit('statement', order)"
              >
                명세서 확인
              </el-button>
            </div>
          </div>

          <div class="card-expanded-body">
            <order-detail-expand :order="order" />

            <div class="order-timeline-section">
              <div class="section-title">공정 및 처리 이력</div>
              <order-status-timeline :order-id="order.id" />
            </div>
          </div>

          <div class="card-bottom-footer">
            <div class="order-product-preview">
              <div v-for="(item, idx) in getSortedItems(order.orderItems).slice(0, 3)" :key="idx" class="preview-item">
                <el-image
                  v-if="item.photoUrl"
                  :src="getThumbnailUrl(item.photoUrl)"
                  fit="cover"
                  class="preview-thumb"
                />
                <span class="preview-name">{{ item.productName || item.productSetTitle }}</span>
              </div>
              <div v-if="order.orderItems.length > 3" class="preview-more">
                외 {{ order.orderItems.length - 3 }}건
              </div>
            </div>

            <div class="price-info-footer">
              <span class="total-label">합계</span>
              <div class="total-amount-display">
                <span class="value">{{ formatPrice(getOrderTotal(order)) }}</span>
                <span class="currency">원</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
    <el-empty v-else description="주문 내역이 없습니다." />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { parseTime, getThumbnailUrl } from '@/utils';
import { formatPrice } from '@/utils/format';
import { isStatementVisibleStatus } from '@/utils/order';
import { Shop, OfficeBuilding } from '@element-plus/icons-vue';
import OrderDetailExpand from '@/components/OrderDetailExpand/index.vue';
import OrderStatusTimeline from '@/components/OrderStatusTimeline/index.vue';

const props = defineProps<{
  orders: any[];
  isMobile: boolean;
  searchTerm?: string;
}>();

const emit = defineEmits(['settle', 'statement', 'cancel']);

const getSortedItems = (orderItems: any[]) => {
  if (!props.searchTerm || props.searchTerm.trim() === '') return orderItems;

  const term = props.searchTerm.toLowerCase().trim();
  return [...orderItems].sort((a, b) => {
    const aMatch = (a.productName?.toLowerCase().includes(term) || a.productNo?.toLowerCase().includes(term) || a.productSetTitle?.toLowerCase().includes(term));
    const bMatch = (b.productName?.toLowerCase().includes(term) || b.productNo?.toLowerCase().includes(term) || b.productSetTitle?.toLowerCase().includes(term));

    if (aMatch && !bMatch) return -1;
    if (!aMatch && bMatch) return 1;
    return 0;
  });
};

const groupedOrders = computed(() => {
  const groups: Record<string, any> = {};

  props.orders.forEach(order => {
    const key = order.groupOrderNo || `SINGLE-${order.orderNo}`;
    if (!groups[key]) {
      groups[key] = {
        key,
        groupOrderNo: order.groupOrderNo,
        createdAt: order.createdAt,
        totalAmount: 0,
        orders: []
      };
    }
    groups[key].orders.push(order);
    groups[key].totalAmount += getOrderTotal(order);

    if (new Date(order.createdAt) > new Date(groups[key].createdAt)) {
      groups[key].createdAt = order.createdAt;
    }
  });

  return Object.values(groups).sort((a: any, b: any) =>
    new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  );
});

const getOrderTotal = (order: any) => {
  return order.orderItems.reduce((sum: number, item: any) => {
    let itemTotal = item.price * item.quantity;
    if (item.children) {
      itemTotal += item.children.reduce((cSum: number, c: any) => cSum + (c.price * c.quantity), 0);
    }
    return sum + itemTotal;
  }, 0);
};

const STATUS_LABEL: Record<string, string> = {

  'ORDERED': '접수대기',

  'Cancelled': '주문취소',

  'LogisticsApproved': '제품준비중',
  'FactoryRequested': '제품준비중',
  'WorkOrderCreated': '제품준비중',
  'InspectedRequested': '제품준비중',
  'REQUEST_CLOSE_BY_AGREEMENT': '제품준비중',
  'CLOSED_BY_AGREEMENT': '제품준비중',
  'Inspected': '제품준비중',

  'PENDING': '정산대기',
  'PROCESSING': '정산중',
  'SETTLED_CANCELLED': '정산취소',

  'SETTLED': '배송중',
  'DELIVERY_READY': '배송중',
  'DELIVERY_IN_TRANSIT': '배송중',
  'DELIVERED': '배송중',

  'Completed': '완료'
};

const getStatusLabel = (status: string): string => STATUS_LABEL[status] ?? status;

const getStatusType = (status: string) => {
  const map: Record<string, string> = {
    'ORDERED': 'info', 
    'Cancelled': 'info', 
    'LogisticsApproved': 'warning', 
    'FactoryRequested': 'warning',
    'WorkOrderCreated': 'warning',
    'InspectedRequested': 'warning',
    'REQUEST_CLOSE_BY_AGREEMENT': 'warning',
    'CLOSED_BY_AGREEMENT': 'warning',
    'Inspected': 'warning',
    'PENDING': 'danger', 
    'PROCESSING': 'warning', 
    'SETTLED_CANCELLED': 'info', 
    'SETTLED': 'primary', 
    'DELIVERY_READY': 'primary',
    'DELIVERY_IN_TRANSIT': 'primary',
    'DELIVERED': 'primary',
    'Completed': 'success' 
  };
  return map[status] ?? 'info';
};

const isStatementVisible = (status: string) => isStatementVisibleStatus(status);
</script>

<style lang="scss" scoped>
.order-list-luxury {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  width: 100%;

  .order-group-container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    width: 100%;
  }

  .order-group-header-luxury {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background-color: #ffffff;
    border: 1px solid #f2efeb;
    border-left: 4px solid #c5a880;
    padding: 1.125rem 1.5rem;
    border-radius: 4px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.01);
    transition: all 0.3s ease;

    :global(html.dark) & {
      background-color: #171719;
      border-color: rgba(255, 255, 255, 0.06);
      border-left-color: #c5a880;
      box-shadow: none;
    }

    &.group-header-compact {
      flex-direction: column;
      align-items: flex-start;
      gap: 1rem;

      .g-info-right {
        width: 100%;
        display: flex;
        justify-content: flex-end;
      }
    }

    .g-info-left {
      display: flex;
      align-items: center;
      gap: 1rem;

      .group-icon-wrapper {
        width: 38px;
        height: 38px;
        border-radius: 50%;
        background-color: rgba(197, 168, 128, 0.08);
        color: #c5a880;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.1rem;

        :global(html.dark) & {
          background-color: rgba(197, 168, 128, 0.15);
        }
      }

      .group-title-meta {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;

        .group-no {
          font-size: 0.95rem;
          font-weight: 800;
          color: #1a1a1a;
          margin: 0;
          letter-spacing: -0.2px;

          :global(html.dark) & {
            color: #f8f9fb;
          }
        }

        .group-date {
          font-size: 0.75rem;
          color: #999999;
          font-family: 'Jost', sans-serif;
        }
      }
    }

    .g-info-right {
      .group-total-info {
        display: flex;
        align-items: center;
        gap: 0.75rem;

        .label {
          font-size: 0.75rem;
          font-weight: 700;
          color: #999999;
          text-transform: uppercase;
          letter-spacing: 0.5px;
        }

        .amount {
          font-size: 1.15rem;
          font-weight: 900;
          color: #c5a880;
          font-family: 'Jost', sans-serif;

          .unit {
            font-size: 0.8rem;
            color: #555555;
            font-weight: 600;
            margin-left: 1px;

            :global(html.dark) & {
              color: #a0a0a0;
            }
          }
        }
      }
    }
  }

  .order-luxury-card {
    background-color: #ffffff;
    border: 1px solid #f2efeb;
    border-radius: 4px;
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.02);
    transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
    overflow: hidden;
    width: 100%;

    :global(html.dark) & {
      background-color: #1e1e20;
      border-color: rgba(255, 255, 255, 0.06);
      box-shadow: none;
    }

    &:hover {
      border-color: #c5a880;
      box-shadow: 0 12px 28px rgba(197, 168, 128, 0.06);
      transform: translateY(-2px);

      :global(html.dark) & {
        box-shadow: 0 12px 28px rgba(0, 0, 0, 0.25);
      }
    }

    .card-top-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid #faf9f6;
      flex-wrap: wrap;
      gap: 1rem;

      :global(html.dark) & {
        border-bottom-color: rgba(255, 255, 255, 0.04);
      }

      .left-meta {
        display: flex;
        align-items: center;
        gap: 0.875rem;
        flex-wrap: wrap;

        .status-indicator-tag {
          border-radius: 4px;
          font-weight: 700;
          letter-spacing: 0.5px;
          padding: 0.25rem 0.875rem;
          border: none;
        }

        .order-id-info {
          display: flex;
          align-items: center;
          gap: 0.75rem;

          .order-no {
            font-size: 0.9rem;
            font-weight: 700;
            color: #2c2c2e;
            font-family: 'Jost', sans-serif;

            :global(html.dark) & {
              color: #eaeaea;
            }
          }

          .user-company-badge {
            font-size: 0.7rem;
            color: #c5a880;
            font-weight: 700;
            background-color: rgba(197, 168, 128, 0.06);
            padding: 3px 8px;
            border-radius: 4px;
            display: inline-flex;
            align-items: center;
            gap: 0.25rem;

            :global(html.dark) & {
              background-color: rgba(197, 168, 128, 0.15);
            }
          }
        }
      }

      .right-actions {
        display: flex;
        align-items: center;
        gap: 0.75rem;

        .action-pill-btn {
          border-radius: 4px;
          font-weight: 700;
          font-size: 0.75rem;
          letter-spacing: 0.5px;
          height: 32px;
          padding: 0 1.125rem;
          transition: all 0.2s ease;
        }

      }
    }

    .card-expanded-body {
      padding: 1.75rem 1.5rem;
      background-color: #ffffff;
      border-bottom: 1px solid #f2efeb;

      :global(html.dark) & {
        background-color: #1a1a1c;
        border-bottom-color: rgba(255, 255, 255, 0.04);
      }

      .order-timeline-section {
        margin-top: 2.25rem;
        padding-top: 1.75rem;
        border-top: 1px dashed #f2efeb;

        :global(html.dark) & {
          border-top-color: rgba(255, 255, 255, 0.06);
        }

        .section-title {
          font-size: 0.85rem;
          font-weight: 800;
          color: #1a1a1a;
          letter-spacing: 1.5px;
          text-transform: uppercase;
          margin-bottom: 1.5rem;
          display: flex;
          align-items: center;

          :global(html.dark) & {
            color: #eaeaea;
          }

          &::before {
            content: '';
            display: inline-block;
            width: 4px;
            height: 12px;
            background-color: #c5a880;
            margin-right: 8px;
            border-radius: 2px;
          }
        }
      }
    }

    .card-bottom-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 1.5rem;
      background-color: #faf9f6;
      flex-wrap: wrap;
      gap: 1rem;

      :global(html.dark) & {
        background-color: #242426;
      }

      .order-product-preview {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        flex-wrap: wrap;

        .preview-item {
          display: flex;
          align-items: center;
          gap: 0.5rem;
          background-color: #ffffff;
          padding: 4px 10px 4px 4px;
          border-radius: 4px;
          border: 1px solid #f2efeb;

          :global(html.dark) & {
            background-color: #1e1e20;
            border-color: rgba(255, 255, 255, 0.06);
          }

          .preview-thumb {
            width: 28px;
            height: 28px;
            border-radius: 4px;
            border: 1px solid #f0eee9;

            :global(html.dark) & {
              border-color: rgba(255, 255, 255, 0.04);
            }
          }

          .preview-name {
            font-size: 0.75rem;
            font-weight: 700;
            color: #555555;
            max-width: 140px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;

            :global(html.dark) & {
              color: #c0c0c0;
            }
          }
        }

        .preview-more {
          font-size: 0.75rem;
          color: #999999;
          font-weight: 700;
          letter-spacing: 0.5px;
        }
      }

      .price-info-footer {
        display: flex;
        align-items: center;
        gap: 0.5rem;

        .total-label {
          font-size: 0.75rem;
          font-weight: 700;
          color: #999999;
          text-transform: uppercase;
        }

        .total-amount-display {
          display: flex;
          align-items: baseline;
          color: #1a1a1a;
          font-family: 'Jost', sans-serif;

          :global(html.dark) & {
            color: #eaeaea;
          }

          .value {
            font-size: 1.15rem;
            font-weight: 800;
          }

          .currency {
            font-size: 0.8rem;
            font-weight: 600;
            margin-left: 2px;
            color: #555555;

            :global(html.dark) & {
              color: #a0a0a0;
            }
          }
        }
      }
    }
  }
}
</style>

